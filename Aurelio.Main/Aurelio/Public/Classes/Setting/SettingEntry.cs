using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.App;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Setting;

public class SettingEntry : ReactiveObject
{
    [Reactive] [JsonProperty] public Enum.Setting.NoticeWay NoticeWay { get; set; } = Enum.Setting.NoticeWay.Bubble;
    [Reactive] [JsonProperty] public Enum.Setting.Theme Theme { get; set; } = Enum.Setting.Theme.Dark;

    [Reactive]
    [JsonProperty]
    public Enum.Minecraft.MinecraftInstanceCategoryMethod
        MinecraftInstanceCategoryMethod { get; set; } = Enum.Minecraft.MinecraftInstanceCategoryMethod.MinecraftVersion;

    [Reactive] [JsonProperty] public string Language { get; set; } = "zh-cn";
    [Reactive] [JsonProperty] public bool EnableIndependentMinecraft { get; set; } = true;

    [Reactive]
    [JsonProperty]
    public ObservableCollection<MinecraftFolderEntry> MinecraftFolderEntries { get; set; } = [];

    [Reactive] [JsonProperty] public ObservableCollection<RecordJavaRuntime> JavaRuntimes { get; set; } = [];
    [Reactive] [JsonProperty] public ObservableCollection<RecordMinecraftAccount> MinecraftAccounts { get; set; } = [];
    [Reactive] [JsonProperty] public RecordMinecraftAccount? UsingMinecraftAccount { get; set; }


    public SettingEntry()
    {
        PropertyChanged += OnPropertyChanged;
        return;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MinecraftInstanceCategoryMethod))
        {
            Module.Service.MinecraftInstances.Categorize(MinecraftInstanceCategoryMethod);
        }

        AppMethod.SaveSetting();
    }
}