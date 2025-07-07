using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Const;
using Aurelio.Public.Enum.Minecraft;
using Aurelio.Public.Langs;
using Avalonia.Threading;
using DynamicData;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;
using SukiUI;

namespace Aurelio.Public.Module.Service;

public class MinecraftInstances
{
    public static void Load(string[] path)
    {
        foreach (var p in path)
        {
            var parser = new MinecraftParser(p);
            Data.AllMinecraftInstances.AddRange(parser.GetMinecrafts()
                .Select(x => new RecordMinecraftEntry(x)));
        }

        Categorize(Data.SettingEntry.MinecraftInstanceCategoryMethod);
    }

    public static void Search(string key, bool ui = true)
    {
        var filtered = Data.SortedMinecraftCategories
            .FirstOrDefault(x => x.Tag == "filtered");
        if (filtered == null) return;
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
        if (Aurelio.App.UiRoot != null)
            Aurelio.App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot.Opacity = 0;
        var filtered = Data.SortedMinecraftCategories
            .FirstOrDefault(x => x.Tag == "filtered")?.Minecrafts;
        Data.SortedMinecraftCategories.Clear();
        Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
        {
            Tag = "filtered",
            Name = MainLang.SearchResult,
            Visible = filtered is { Count: > 0 },
            Expanded = filtered is { Count: > 0 },
            Minecrafts = filtered ?? []
        });
        switch (method)
        {
            case MinecraftInstanceCategoryMethod.None:
                Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
                {
                    Tag = "all",
                    Name = MainLang.AllInstance,
                    Minecrafts = new ObservableCollection<RecordMinecraftEntry>(Data.AllMinecraftInstances.ToList()),
                    Expanded = true
                });
                break;

            case MinecraftInstanceCategoryMethod.Tag:
                var allTags = Data.AllMinecraftInstances
                    .SelectMany(minecraft => minecraft.Tags)
                    .Distinct()
                    .OrderBy(tag => tag);

                foreach (var tag in allTags)
                {
                    var minecraftsWithTag = Data.AllMinecraftInstances
                        .Where(minecraft => minecraft.Tags.Contains(tag))
                        .ToList();

                    if (minecraftsWithTag.Any())
                    {
                        Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
                        {
                            Tag = $"tag-{tag}",
                            Name = tag,
                            Minecrafts = new ObservableCollection<RecordMinecraftEntry>(minecraftsWithTag),
                            Expanded = true
                        });
                    }
                }

                var minecraftsWithoutTag = new ObservableCollection<RecordMinecraftEntry>
                    (Data.AllMinecraftInstances.Where(x => x.Tags.Length == 0));
                Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
                {
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
                        .ToList();

                    if (minecraftsWithLoader.Any())
                    {
                        Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
                        {
                            Tag = loader,
                            Name = loader,
                            Minecrafts = new ObservableCollection<RecordMinecraftEntry>(minecraftsWithLoader),
                            Expanded = true
                        });
                    }
                }

                break;

            case MinecraftInstanceCategoryMethod.MinecraftVersion:
                // 首先按VersionType分类
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
                                {
                                    majorVersions[majorVersion] = new List<RecordMinecraftEntry>();
                                }

                                majorVersions[majorVersion].Add(minecraft);
                            }
                            else
                            {
                                // 对于不符合主版本号格式的，单独添加
                                Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
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
                            Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
                            {
                                Tag = $"release-{majorVersion}",
                                Name = $"Minecraft {majorVersion}",
                                Minecrafts =
                                    new ObservableCollection<RecordMinecraftEntry>(majorVersions[majorVersion]),
                                Expanded = true
                            });
                        }
                    }
                    else
                    {
                        // 对于非Release版本，直接添加
                        Data.SortedMinecraftCategories.Add(new MinecraftCategoryEntry()
                        {
                            Tag = versionTypeName.ToLower(),
                            Name = versionTypeName,
                            Minecrafts = new ObservableCollection<RecordMinecraftEntry>(minecraftsInGroup),
                            Expanded = true
                        });
                    }
                }

                break;
        }

        if (Aurelio.App.UiRoot != null)
            Aurelio.App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot
                .Animate<double>(Visual.OpacityProperty, 0, 1);
    }
}