using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Services;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Setting;

public class SettingEntry : ReactiveObject
{
    [Reactive] [JsonProperty] public Enum.Setting.NoticeWay NoticeWay { get; set; } = Enum.Setting.NoticeWay.Bubble;
    [Reactive] [JsonProperty] public Enum.Setting.Theme Theme { get; set; } = Enum.Setting.Theme.Dark;
    [Reactive] [JsonProperty] public double MemoryLimit { get; set; } = 2048;

    [Reactive]
    [JsonProperty]
    public Enum.Minecraft.MinecraftInstanceCategoryMethod
        MinecraftInstanceCategoryMethod { get; set; } = Enum.Minecraft.MinecraftInstanceCategoryMethod.MinecraftVersion;

    [Reactive]
    [JsonProperty]
    public Enum.Minecraft.MinecraftInstanceSortMethod
        MinecraftInstanceSortMethod { get; set; } = Enum.Minecraft.MinecraftInstanceSortMethod.Name;

    [Reactive] [JsonProperty] public string Language { get; set; } = "zh-cn";
    [Reactive] [JsonProperty] public bool UseFilePicker { get; set; } = true;
    [Reactive] [JsonProperty] public bool EnableIndependentMinecraft { get; set; } = true;


    [Reactive]
    [JsonProperty]
    public RecordJavaRuntime PreferredJavaRuntime { get; set; } = new() { JavaVersion = "auto" };


    [Reactive]
    [JsonProperty]
    public ObservableCollection<RecordMinecraftFolderEntry> MinecraftFolderEntries { get; set; } = [];

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
            if (App.UiRoot == null) return;
            App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot.Opacity = 0;
            MinecraftInstances.Categorize(MinecraftInstanceCategoryMethod);
        }

        if (e.PropertyName == nameof(MinecraftInstanceSortMethod))
        {
            if (App.UiRoot == null) return;
            App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot.Opacity = 0;
            MinecraftInstances.Sort(MinecraftInstanceSortMethod);
        }


        AppMethod.SaveSetting();
    }
}