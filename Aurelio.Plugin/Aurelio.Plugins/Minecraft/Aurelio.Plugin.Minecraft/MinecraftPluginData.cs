using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;

namespace Aurelio.Plugin.Minecraft;

public sealed class MinecraftPluginData : INotifyPropertyChanged
{
    private static MinecraftPluginData? _instance;

    // Private fields for reactive properties
    private bool _isRender3D;
    private bool _isLoadingMinecraftLoading;

    public static MinecraftPluginData Instance
    {
        get { return _instance ??= new MinecraftPluginData(); }
    }

    public static List<RecordMinecraftEntry> AllMinecraftInstances { get; } = [];
    public static ObservableCollection<MinecraftCategoryEntry> SortedMinecraftCategories { get; } = [];
    public static MinecraftPluginSettingEntry MinecraftPluginSettingEntry { get; set; }
    public static ObservableCollection<string> AllMinecraftTags { get; } = [];

    // Properties with INotifyPropertyChanged implementation
    public bool IsRender3D
    {
        get => _isRender3D;
        set => SetProperty(ref _isRender3D, value);
    }

    public bool IsLoadingMinecraftLoading
    {
        get => _isLoadingMinecraftLoading;
        set => SetProperty(ref _isLoadingMinecraftLoading, value);
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
}