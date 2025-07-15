using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Classes.Setting;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Const;

public class Data : ReactiveObject
{
    private static Data? _instance;

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }

    public static DesktopType DesktopType { get; set; } = DesktopType.Unknown;
    public static SettingEntry SettingEntry { get; set; }
    public static UiProperty UiProperty { get; set; } = UiProperty.Instance;
    public static string TranslateToken { get; set; }
    [Reactive] public string Version { get; set; }
    public static List<RecordMinecraftEntry> AllMinecraftInstances { get; } = [];
    public static ObservableCollection<MinecraftCategoryEntry> SortedMinecraftCategories { get; } = [];
}