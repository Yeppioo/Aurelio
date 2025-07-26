using System.ComponentModel;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.IO.Local;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DynamicData;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MinecraftClientLauncher = Aurelio.Plugin.Minecraft.Service.Minecraft.Launcher.MinecraftClientLauncher;
using MinecraftInstanceIconType = Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft.MinecraftInstanceIconType;
using MinecraftInstancesHandler = Aurelio.Plugin.Minecraft.Service.Minecraft.MinecraftInstancesHandler;
using MinecraftSpecialFolder = Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft.MinecraftSpecialFolder;

namespace Aurelio.Plugin.Minecraft.Classes.Minecraft;

public class RecordMinecraftEntry : ReactiveObject
{
    private readonly Debouncer _debouncer;

    public RecordMinecraftEntry(MinecraftEntry mlEntry)
    {
        MlEntry = mlEntry;
        Id = mlEntry.Id;
        Loader = mlEntry.IsVanilla
            ? "Vanilla"
            : string.Join(", ", (mlEntry as ModifiedMinecraftEntry)?
                .ModLoaders.Select(x => $"{x.Type}")!);
        Loaders = mlEntry.IsVanilla
            ? ["Vanilla"]
            : (mlEntry as ModifiedMinecraftEntry)?
            .ModLoaders.Select(x => $"{x.Type}").ToArray();
        SettingEntry = GetMinecraftSetting();
        Icon = Calculator.GetMinecraftInstanceIcon(this);
        _debouncer = new Debouncer(() =>
        {
            var path = Path.Combine(Calculator.GetMinecraftSpecialFolder
                (MlEntry, MinecraftSpecialFolder.InstanceFolder), "Aurelio.MinecraftInstance.Setting.Yeppioo");
            File.WriteAllText(path, SettingEntry.AsJson());
        }, 250);
        SettingEntry.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(SettingEntry.IconType) or nameof(SettingEntry.IconData))
            {
                Icon = Calculator.GetMinecraftInstanceIcon(this);
                SetIcon((s as Control)!, e);
            }

            _debouncer.Trigger();
        };
        SettingEntry.Tags.CollectionChanged += (_, _) =>
        {
            _debouncer.Trigger();
            // 不要重新加载所有实例，只更新标签列表
            MinecraftPluginData.AllMinecraftTags.Clear();

            // 添加所有用户定义的标签
            var userTags = MinecraftPluginData.AllMinecraftInstances
                .SelectMany(minecraft => minecraft.SettingEntry.Tags)
                .Distinct()
                .OrderBy(tag => tag);

            MinecraftPluginData.AllMinecraftTags.AddRange(userTags);
            // 只需重新分类，不需要重新加载
            MinecraftInstancesHandler.Categorize(MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftInstanceCategoryMethod);
        };

        // 监听收藏状态变化，触发重新分类
        SettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingEntry.IsFavourite))
            {
                _debouncer.Trigger();
                MinecraftInstancesHandler.Categorize(MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftInstanceCategoryMethod);
            }
        };
    }

    [Reactive] public string Id { get; set; }
    public MinecraftVersionType Type => MlEntry.Version.Type;
    public MinecraftEntry MlEntry { get; }
    public string ShortDescription => $"{Loader} {MlEntry.Version.VersionId}";
    public string Loader { get; }
    public string[] Loaders { get; }
    [Reactive] public Bitmap Icon { get; set; }
    [Reactive] public object Tag { get; set; }
    public MinecraftInstanceSettingEntry SettingEntry { get; }

    public RecordMinecraftFolderEntry? ParentMinecraftFolder =>
        Calculator.GetMinecraftFolderByEntry(MlEntry);

    public string InstancePath =>
        Calculator.GetMinecraftSpecialFolder(MlEntry, MinecraftSpecialFolder.InstanceFolder);

    public void Launch(Control sender)
    {
        _ = MinecraftClientLauncher.Launch(this, sender);
    }

    private MinecraftInstanceSettingEntry GetMinecraftSetting()
    {
        var path = Path.Combine(Calculator.GetMinecraftSpecialFolder
            (MlEntry, MinecraftSpecialFolder.InstanceFolder), "Aurelio.MinecraftInstance.Setting.Yeppioo");
        if (File.Exists(path))
            return JsonConvert.DeserializeObject<MinecraftInstanceSettingEntry>(File.ReadAllText(path));
        File.WriteAllText(path, new MinecraftInstanceSettingEntry().AsJson());
        return new MinecraftInstanceSettingEntry();
    }

    private async void SetIcon(Control sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MinecraftInstanceSettingEntry.IconType) ||
            SettingEntry.IconType != MinecraftInstanceIconType.Base64) return;
        var pic = await sender.PickFileAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = MainLang.SelectImgFile,
            FileTypeFilter =
                [new FilePickerFileType("Image Files") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"] }]
        });
        if (pic.Count == 0) return;
        SettingEntry.IconData = Convert.ToBase64String(await File.ReadAllBytesAsync(pic[0]));
    }
}