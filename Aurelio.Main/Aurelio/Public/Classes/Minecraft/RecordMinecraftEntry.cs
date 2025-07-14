using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Service.Minecraft;
using Aurelio.Public.Module.Service.Minecraft.Launcher;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DynamicData;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Minecraft;

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
        SettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SettingEntry.IconType) or nameof(SettingEntry.IconData))
            {
                Icon = Calculator.GetMinecraftInstanceIcon(this);
                SetIcon(e);
            }

            _debouncer.Trigger();
        };
        SettingEntry.Tags.CollectionChanged += (_, _) =>
        {
            _debouncer.Trigger();
            // 不要重新加载所有实例，只更新标签列表
            UiProperty.AllMinecraftTags.Clear();
            
            // 先添加内置标签
            UiProperty.AllMinecraftTags.AddRange(UiProperty.BuiltInTags);
            
            // 再添加用户标签（避免重复添加内置标签）
            var userTags = Data.AllMinecraftInstances
                .SelectMany(minecraft => minecraft.SettingEntry.Tags)
                .Where(tag => !UiProperty.BuiltInTags.Contains(tag))
                .Distinct()
                .OrderBy(tag => tag);
                
            UiProperty.AllMinecraftTags.AddRange(userTags);
            // 只需重新分类，不需要重新加载
            HandleInstances.Categorize(Data.SettingEntry.MinecraftInstanceCategoryMethod);
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

    public void Launch()
    {
        _ = MinecraftClientLauncher.Launch(this);
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

    private async void SetIcon(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MinecraftInstanceSettingEntry.IconType) ||
            SettingEntry.IconType != MinecraftInstanceIconType.Base64) return;
        var pic = await App.TopLevel.StorageProvider.PickFileAsync(new FilePickerOpenOptions
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