using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Public.Module;
using Aurelio.Public.Module.App;
using Newtonsoft.Json;
using MinecraftInstancesHandler = Aurelio.Plugin.Minecraft.Service.Minecraft.MinecraftInstancesHandler;

namespace Aurelio.Plugin.Minecraft;

public sealed class MinecraftPluginSettingEntry : INotifyPropertyChanged
{
    // Private fields
    private RecordJavaRuntime _preferredJavaRuntime = new() { JavaVersion = "auto" };
    private ObservableCollection<RecordMinecraftFolderEntry> _minecraftFolderEntries = [];
    private ObservableCollection<RecordJavaRuntime> _javaRuntimes = [];
    private ObservableCollection<RecordMinecraftAccount> _minecraftAccounts = [];
    private RecordMinecraftAccount? _usingMinecraftAccount;
    private double _memoryLimit = 2048;
    private MinecraftInstanceCategoryMethod _minecraftInstanceCategoryMethod = MinecraftInstanceCategoryMethod.MinecraftVersion;
    private MinecraftInstanceSortMethod _minecraftInstanceSortMethod = MinecraftInstanceSortMethod.Name;
    private Classes.Enum.Setting.WindowVisibility _windowVisibility = Classes.Enum.Setting.WindowVisibility.AfterLaunchKeepVisible;
    private bool _enableIndependentMinecraft = true;

    // Properties
    [JsonProperty]
    public RecordJavaRuntime PreferredJavaRuntime
    {
        get => _preferredJavaRuntime;
        set => SetProperty(ref _preferredJavaRuntime, value);
    }

    [JsonProperty]
    public ObservableCollection<RecordMinecraftFolderEntry> MinecraftFolderEntries
    {
        get => _minecraftFolderEntries;
        set => SetProperty(ref _minecraftFolderEntries, value);
    }

    [JsonProperty]
    public ObservableCollection<RecordJavaRuntime> JavaRuntimes
    {
        get => _javaRuntimes;
        set => SetProperty(ref _javaRuntimes, value);
    }

    [JsonProperty]
    public ObservableCollection<RecordMinecraftAccount> MinecraftAccounts
    {
        get => _minecraftAccounts;
        set => SetProperty(ref _minecraftAccounts, value);
    }

    [JsonProperty]
    public RecordMinecraftAccount? UsingMinecraftAccount
    {
        get => _usingMinecraftAccount;
        set => SetProperty(ref _usingMinecraftAccount, value);
    }

    [JsonProperty]
    public double MemoryLimit
    {
        get => _memoryLimit;
        set => SetProperty(ref _memoryLimit, value);
    }

    [JsonProperty]
    public MinecraftInstanceCategoryMethod MinecraftInstanceCategoryMethod
    {
        get => _minecraftInstanceCategoryMethod;
        set => SetProperty(ref _minecraftInstanceCategoryMethod, value);
    }

    [JsonProperty]
    public MinecraftInstanceSortMethod MinecraftInstanceSortMethod
    {
        get => _minecraftInstanceSortMethod;
        set => SetProperty(ref _minecraftInstanceSortMethod, value);
    }

    [JsonProperty]
    public Classes.Enum.Setting.WindowVisibility WindowVisibility
    {
        get => _windowVisibility;
        set => SetProperty(ref _windowVisibility, value);
    }

    [JsonProperty]
    public bool EnableIndependentMinecraft
    {
        get => _enableIndependentMinecraft;
        set => SetProperty(ref _enableIndependentMinecraft, value);
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public MinecraftPluginSettingEntry()
    {
        PropertyChanged += OnPropertyChangedHandler;
    }

    private void OnPropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
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