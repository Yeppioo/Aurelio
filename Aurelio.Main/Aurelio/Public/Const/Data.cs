using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Enum;
using ReactiveUI;

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
    public static ObservableCollection<MinecraftCategoryEntry> MinecraftInstances { get; set; } = [];
}