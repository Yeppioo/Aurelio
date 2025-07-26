using System.Collections.ObjectModel;
using System.ComponentModel;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Public.Module;
using Aurelio.Public.Module.App;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MinecraftInstancesHandler = Aurelio.Plugin.Minecraft.Service.Minecraft.MinecraftInstancesHandler;

namespace Aurelio.Plugin.Minecraft;

public class MinecraftPluginSettingEntry : ReactiveObject
{
    [Reactive]
    [JsonProperty]
    public RecordJavaRuntime PreferredJavaRuntime { get; set; } = new() { JavaVersion = "auto" };

    [Reactive]
    [JsonProperty]
    public ObservableCollection<RecordMinecraftFolderEntry> MinecraftFolderEntries { get; set; } = [];

    [Reactive] [JsonProperty] public ObservableCollection<RecordJavaRuntime> JavaRuntimes { get; set; } = [];
    [Reactive] [JsonProperty] public ObservableCollection<RecordMinecraftAccount> MinecraftAccounts { get; set; } = [];
    [Reactive] [JsonProperty] public RecordMinecraftAccount? UsingMinecraftAccount { get; set; }

    [Reactive] [JsonProperty] public double MemoryLimit { get; set; } = 2048;

    [Reactive]
    [JsonProperty]
    public MinecraftInstanceCategoryMethod
        MinecraftInstanceCategoryMethod { get; set; } = MinecraftInstanceCategoryMethod.MinecraftVersion;

    [Reactive]
    [JsonProperty]
    public MinecraftInstanceSortMethod
        MinecraftInstanceSortMethod { get; set; } = MinecraftInstanceSortMethod.Name;

    [Reactive]
    [JsonProperty]
    public Classes.Enum.Setting.WindowVisibility WindowVisibility { get; set; } =
        Classes.Enum.Setting.WindowVisibility.AfterLaunchKeepVisible;

    [Reactive] [JsonProperty] public bool EnableIndependentMinecraft { get; set; } = true;

    public MinecraftPluginSettingEntry()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MinecraftInstanceCategoryMethod))
        {
            if (App.UiRoot == null) return;
            MinecraftInstancesHandler.Categorize(MinecraftInstanceCategoryMethod);
        }
        else if (e.PropertyName == nameof(MinecraftInstanceSortMethod))
        {
            if (App.UiRoot == null) return;
            MinecraftInstancesHandler.Sort(MinecraftInstanceSortMethod);
        }

        AppMethod.SaveSetting();
    }
}