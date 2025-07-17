using System.Collections.ObjectModel;
using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Service.Minecraft;
using Aurelio.Public.Module.Ui;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Setting;

public class SettingEntry : ReactiveObject
{
    public SettingEntry()
    {
        PropertyChanged += OnPropertyChanged;
        MinecraftAccounts.CollectionChanged += (_, _) => Data.UpdateAggregateSearchEntries();
    }

    [Reactive] [JsonProperty] public Enum.Setting.NoticeWay NoticeWay { get; set; } = Enum.Setting.NoticeWay.Bubble;
    [Reactive] [JsonProperty] public Enum.Setting.Theme Theme { get; set; } = Enum.Setting.Theme.Dark;
    [Reactive] [JsonProperty] public double MemoryLimit { get; set; } = 2048;

    [Reactive]
    [JsonProperty]
    public MinecraftInstanceCategoryMethod
        MinecraftInstanceCategoryMethod { get; set; } = MinecraftInstanceCategoryMethod.MinecraftVersion;

    [Reactive]
    [JsonProperty]
    public MinecraftInstanceSortMethod
        MinecraftInstanceSortMethod { get; set; } = MinecraftInstanceSortMethod.Name;

    [Reactive] [JsonProperty] public Language Language { get; set; } = LanguageTypes.Langs[0];
    [Reactive] [JsonProperty] public bool UseFilePicker { get; set; } = true;
    [Reactive] [JsonProperty] public bool AutoCheckUpdate { get; set; } = true;
    [Reactive] [JsonProperty] public bool EnableSpeedUpGithubApi { get; set; } = true;
    [Reactive] [JsonProperty] public bool EnableIndependentMinecraft { get; set; } = true;

    [Reactive] [JsonProperty] public string GithubSpeedUpApiUrl { get; set; } = "https://ghproxy.net/%url%";

    [Reactive]
    [JsonProperty]
    public RecordJavaRuntime PreferredJavaRuntime { get; set; } = new() { JavaVersion = "auto" };


    [Reactive]
    [JsonProperty]
    public ObservableCollection<RecordMinecraftFolderEntry> MinecraftFolderEntries { get; set; } = [];

    [Reactive] [JsonProperty] public ObservableCollection<RecordJavaRuntime> JavaRuntimes { get; set; } = [];
    [Reactive] [JsonProperty] public ObservableCollection<RecordMinecraftAccount> MinecraftAccounts { get; set; } = [];
    [Reactive] [JsonProperty] public RecordMinecraftAccount? UsingMinecraftAccount { get; set; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MinecraftInstanceCategoryMethod))
        {
            if (App.UiRoot == null) return;
            App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot.Opacity = 0;
            HandleMinecraftInstances.Categorize(MinecraftInstanceCategoryMethod);
        }

        if (e.PropertyName == nameof(MinecraftInstanceSortMethod))
        {
            if (App.UiRoot == null) return;
            App.UiRoot.ViewModel.HomeTabPage.MinecraftCardsContainerRoot.Opacity = 0;
            HandleMinecraftInstances.Sort(MinecraftInstanceSortMethod);
        }
        
        if (e.PropertyName == nameof(Theme))
        {
            Setter.ToggleTheme(Theme);
        }


        AppMethod.SaveSetting();
    }
}