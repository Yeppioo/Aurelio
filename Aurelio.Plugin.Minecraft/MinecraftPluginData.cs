using System.Collections.ObjectModel;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Plugin.Minecraft;

public class MinecraftPluginData : ReactiveObject
{
    private static MinecraftPluginData? _instance;

    public static MinecraftPluginData Instance
    {
        get { return _instance ??= new MinecraftPluginData(); }
    }
    
    public static List<RecordMinecraftEntry> AllMinecraftInstances { get; } = [];
    public static ObservableCollection<MinecraftCategoryEntry> SortedMinecraftCategories { get; } = [];
    public static MinecraftPluginSettingEntry MinecraftPluginSettingEntry { get; set; }
    public static ObservableCollection<string> AllMinecraftTags { get; } = [];
    [Reactive] public bool IsRender3D { get; set; }
    [Reactive] public bool IsLoadingMinecraftLoading { get; set; }
}