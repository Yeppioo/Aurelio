using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Avalonia.Threading;
using DynamicData;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Components.Parser;

namespace Aurelio.Public.Module.Service.Minecraft;

public partial class HandleMinecraftInstances
{
    private static CancellationTokenSource? _cts;

    public static async Task Load(string[] path)
    {
        Aurelio.App.UiRoot.ViewModel.HomeTabPage.Root.IsVisible = false;
        Aurelio.App.UiRoot.ViewModel.HomeTabPage.ProgressRing.IsVisible = true;
        await Task.Run(() =>
        {
            // 清空现有实例集合，避免重复加载
            Data.AllMinecraftInstances.Clear();

            foreach (var p in path)
            {
                var parser = new MinecraftParser(p);
                Data.AllMinecraftInstances.AddRange(parser.GetMinecrafts()
                    .Select(x => new RecordMinecraftEntry(x)));
            }



            // 清除所有实例中的重复标签
            foreach (var instance in Data.AllMinecraftInstances) instance.SettingEntry.RemoveDuplicateTags();

            // 更新标签列表，只包含用户定义的标签
            UiProperty.AllMinecraftTags.Clear();

            // 添加所有用户定义的标签
            var userTags = Data.AllMinecraftInstances
                .SelectMany(minecraft => minecraft.SettingEntry.Tags)
                .Distinct()
                .OrderBy(tag => tag);

            UiProperty.AllMinecraftTags.AddRange(userTags);
            Data.UpdateAggregateSearchEntries();
        });
        Categorize(Data.SettingEntry.MinecraftInstanceCategoryMethod);
        Aurelio.App.UiRoot.ViewModel.HomeTabPage.Root.IsVisible = true;
        Aurelio.App.UiRoot.ViewModel.HomeTabPage.ProgressRing.IsVisible = false;
    }

    public static void Sort(MinecraftInstanceSortMethod method)
    {
        foreach (var category in Data.SortedMinecraftCategories)
        {
            if (category.Minecrafts is not { Count: > 1 })
                continue;

            List<RecordMinecraftEntry> sortedList;

            switch (method)
            {
                case MinecraftInstanceSortMethod.Name:
                    // 按名称字母顺序排序
                    sortedList = category.Minecrafts.OrderBy(x => x.Id).ToList();
                    break;

                case MinecraftInstanceSortMethod.LastPlayed:
                    // 按最后游玩时间排序，最近的在前面，如果时间一样则按名称排序
                    sortedList = category.Minecrafts
                        .OrderByDescending(x => x.SettingEntry?.LastPlayed ?? DateTime.MinValue)
                        .ThenBy(x => x.Id)
                        .ToList();
                    break;

                case MinecraftInstanceSortMethod.MinecraftVersion:
                    // 按游戏版本排序，高版本在前面
                    sortedList = category.Minecrafts
                        .OrderBy(x =>
                        {
                            try
                            {
                                // 提取版本号
                                var versionId = x.MlEntry.Version.VersionId ?? "";
                                var match = MyRegex().Match(versionId);

                                if (match.Success)
                                {
                                    // 解析版本号组件
                                    var major = int.Parse(match.Groups[1].Value);
                                    var minor = int.Parse(match.Groups[2].Value);
                                    var patch = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

                                    // 使用元组而非匿名类型（元组可比较）
                                    return (-major, -minor, -patch);
                                }

                                // 无法解析的版本排在后面
                                return (int.MaxValue, int.MaxValue, int.MaxValue);
                            }
                            catch
                            {
                                // 错误处理，确保即使有异常也能返回有效值
                                return (int.MaxValue, int.MaxValue, int.MaxValue);
                            }
                        })
                        .ThenBy(x => x.Id)
                        .ToList();
                    break;

                default:
                    continue;
            }

            // 更新分类中的实例列表
            category.Minecrafts.Clear();
            foreach (var item in sortedList) category.Minecrafts.Add(item);
        }

        if (Aurelio.App.UiRoot == null) return;
        if (_cts is { IsCancellationRequested: false })
            _cts.Cancel();
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _cts = Aurelio.App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot
                .Animate<double>(Visual.OpacityProperty, 0, 1);
        });
    }

    public static void Search(string key, bool ui = true)
    {
        var filtered = Data.SortedMinecraftCategories
            .FirstOrDefault(x => x.Tag == "filtered");
        if (filtered == null) return;
        filtered.Minecrafts.Clear();
        filtered.Expanded = false;
        if (ui && key.IsNullOrWhiteSpace())
        {
            filtered.Visible = false;
            return;
        }

        filtered.Minecrafts.Clear();
        var filteredItems = Data.AllMinecraftInstances.Where(item =>
            item.Id.Contains(key, StringComparison.OrdinalIgnoreCase) ||
            item.ShortDescription.Contains(key, StringComparison.OrdinalIgnoreCase));
        filtered.Minecrafts.AddRange(filteredItems);
        if (!ui) return;
        filtered.Visible = true;
        filtered.Expanded = true;
    }

    public static void Categorize(MinecraftInstanceCategoryMethod method)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Aurelio.App.UiRoot != null)
                Aurelio.App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot.Opacity = 0;
        });
        var filtered = Data.SortedMinecraftCategories
            .FirstOrDefault(x => x.Tag == "filtered")?.Minecrafts;
        Data.SortedMinecraftCategories.Clear();

        var categories = new List<MinecraftCategoryEntry>();

        // 获取收藏的实例（使用新的布尔属性）
        var favouriteInstances = Data.AllMinecraftInstances
            .Where(minecraft => minecraft.SettingEntry.IsFavourite)
            .OrderBy(x => x.Id)
            .ToList();

        if (favouriteInstances.Any())
            categories.Add(new MinecraftCategoryEntry
            {
                Tag = "favourites",
                Name = MainLang.Favourites, // 使用本地化显示名称
                Minecrafts = new ObservableCollection<RecordMinecraftEntry>(favouriteInstances),
                Expanded = true
            });

        // 先添加搜索结果分类（绝对置顶）
        Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry
        {
            Tag = "filtered",
            Name = MainLang.SearchResult,
            Visible = filtered is { Count: > 0 },
            Expanded = filtered is { Count: > 0 },
            Minecrafts = filtered ?? []
        });

        // 再添加收藏夹和其他分类
        foreach (var category in categories) Data.SortedMinecraftCategories.Add(category);

        switch (method)
        {
            case MinecraftInstanceCategoryMethod.None:
                // 显示所有实例，不排除收藏夹的实例
                var allInstances = Data.AllMinecraftInstances
                    .OrderBy(x => x.Id)
                    .ToList();

                categories.Add(new MinecraftCategoryEntry
                {
                    Tag = "all",
                    Name = MainLang.AllInstance,
                    Minecrafts = new ObservableCollection<RecordMinecraftEntry>(allInstances),
                    Expanded = true
                });
                break;

            case MinecraftInstanceCategoryMethod.Tag:
                var sortedTags = UiProperty.AllMinecraftTags
                    .OrderBy(tag => tag);

                foreach (var tag in sortedTags)
                {
                    var minecraftsWithTag = Data.AllMinecraftInstances
                        .Where(minecraft => minecraft.SettingEntry.Tags.Contains(tag))
                        .OrderBy(x => x.Id)
                        .ToList();

                    if (minecraftsWithTag.Any())
                        categories.Add(new MinecraftCategoryEntry
                        {
                            Tag = $"tag-{tag}",
                            Name = tag,
                            Minecrafts = new ObservableCollection<RecordMinecraftEntry>(minecraftsWithTag),
                            Expanded = true
                        });
                }

                // 最后处理未分类的实例
                var minecraftsWithoutTag = new ObservableCollection<RecordMinecraftEntry>(
                    Data.AllMinecraftInstances
                        .Where(x => x.SettingEntry.Tags.Count == 0)
                        .OrderBy(x => x.Id)
                        .ToList());
                categories.Add(new MinecraftCategoryEntry
                {
                    Tag = "unclassified",
                    Name = MainLang.Unclassified,
                    Minecrafts = minecraftsWithoutTag,
                    Expanded = true,
                    Visible = minecraftsWithoutTag.Count > 0
                });
                break;

            case MinecraftInstanceCategoryMethod.Loaders:
                var allLoaders = Data.AllMinecraftInstances
                    .SelectMany(minecraft => minecraft.Loaders)
                    .Distinct()
                    .OrderBy(loader => loader);

                foreach (var loader in allLoaders)
                {
                    var minecraftsWithLoader = Data.AllMinecraftInstances
                        .Where(minecraft => minecraft.Loaders.Contains(loader))
                        .OrderBy(x => x.Id)
                        .ToList();

                    if (minecraftsWithLoader.Any())
                        categories.Add(new MinecraftCategoryEntry
                        {
                            Tag = loader,
                            Name = loader,
                            Minecrafts = new ObservableCollection<RecordMinecraftEntry>(minecraftsWithLoader),
                            Expanded = true
                        });
                }

                break;

            case MinecraftInstanceCategoryMethod.FolderName:
                // 按文件夹名称分类，不排除收藏夹的实例
                var folderGroups = Data.AllMinecraftInstances
                    .GroupBy(minecraft =>
                    {
                        var folder = Calculator.GetMinecraftFolderByEntry(minecraft.MlEntry);
                        return folder?.Name ?? MainLang.Unclassified;
                    })
                    .OrderBy(group => group.Key);

                foreach (var folderGroup in folderGroups)
                {
                    // 按名称排序文件夹内的实例
                    var minecraftsInFolder = folderGroup
                        .OrderBy(x => x.Id)
                        .ToList();

                    if (minecraftsInFolder.Any())
                        categories.Add(new MinecraftCategoryEntry
                        {
                            Tag = $"folder-{folderGroup.Key}",
                            Name = folderGroup.Key,
                            Minecrafts = new ObservableCollection<RecordMinecraftEntry>(minecraftsInFolder),
                            Expanded = true
                        });
                }

                break;

            case MinecraftInstanceCategoryMethod.MinecraftVersion:
                // 首先按VersionType分类，不排除收藏夹的实例
                var versionGroups = Data.AllMinecraftInstances
                    .GroupBy(minecraft => minecraft.Type)
                    .OrderBy(group => group.Key);

                foreach (var versionGroup in versionGroups)
                {
                    var versionTypeName = versionGroup.Key.ToString();
                    var minecraftsInGroup = versionGroup.ToList();

                    // 对于Release版本，进一步按主版本号分组
                    if (versionGroup.Key == MinecraftVersionType.Release)
                    {
                        // 获取所有主版本号（如1.12, 1.16等）
                        var majorVersions = new Dictionary<string, List<RecordMinecraftEntry>>();
                        var regex = new Regex(@"^(\d+\.\d+)");

                        foreach (var minecraft in minecraftsInGroup)
                        {
                            var match = regex.Match(minecraft.MlEntry.Version.VersionId);
                            if (match.Success)
                            {
                                var majorVersion = match.Groups[1].Value;
                                if (!majorVersions.ContainsKey(majorVersion))
                                    majorVersions[majorVersion] = new List<RecordMinecraftEntry>();

                                majorVersions[majorVersion].Add(minecraft);
                            }
                            else
                            {
                                // 对于不符合主版本号格式的，单独添加
                                categories.Add(new MinecraftCategoryEntry
                                {
                                    Tag = minecraft.MlEntry.Version.VersionId,
                                    Name = $"Release - {minecraft.MlEntry.Version.VersionId}",
                                    Minecrafts = new ObservableCollection<RecordMinecraftEntry>(new[] { minecraft }),
                                    Expanded = true
                                });
                            }
                        }

                        // 添加按主版本号分组的分类，确保正确的版本号排序（从新到旧）
                        var sortedVersions = majorVersions.Keys.Select(v =>
                            {
                                var parts = v.Split('.');
                                return new
                                {
                                    VersionString = v,
                                    Major = int.Parse(parts[0]),
                                    Minor = int.Parse(parts[1])
                                };
                            })
                            .OrderByDescending(v => v.Major)
                            .ThenByDescending(v => v.Minor)
                            .Select(v => v.VersionString);

                        foreach (var majorVersion in sortedVersions)
                        {
                            // 按名称排序版本内的实例
                            var sortedInstances = majorVersions[majorVersion]
                                .OrderBy(x => x.Id)
                                .ToList();

                            categories.Add(new MinecraftCategoryEntry
                            {
                                Tag = $"release-{majorVersion}",
                                Name = $"Minecraft {majorVersion}",
                                Minecrafts = new ObservableCollection<RecordMinecraftEntry>(sortedInstances),
                                Expanded = true
                            });
                        }
                    }
                    else
                    {
                        // 对于非Release版本，直接添加，并按名称排序
                        var sortedInstances = minecraftsInGroup
                            .OrderBy(x => x.Id)
                            .ToList();

                        categories.Add(new MinecraftCategoryEntry
                        {
                            Tag = versionTypeName.ToLower(),
                            Name = versionTypeName,
                            Minecrafts = new ObservableCollection<RecordMinecraftEntry>(sortedInstances),
                            Expanded = true
                        });
                    }
                }

                break;
        }

        foreach (var category in categories)
        {
            if (category.Tag == "favourites") continue;
            Data.SortedMinecraftCategories.Add(category);
        }

        if (Data.SettingEntry == null) return;
        if (Data.SettingEntry.MinecraftInstanceSortMethod != MinecraftInstanceSortMethod.Name)
            Sort(Data.SettingEntry.MinecraftInstanceSortMethod);
        else if (Aurelio.App.UiRoot != null)
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Aurelio.App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot
                    .Animate<double>(Visual.OpacityProperty, 0, 1);
            });
    }



    [GeneratedRegex(@"^(\d+)\.(\d+)(?:\.(\d+))?")]
    private static partial Regex MyRegex();
}