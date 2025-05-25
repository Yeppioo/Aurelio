using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
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
    public static SettingEntry? SettingEntry { get; set; }

    [Reactive]
    public AccountEntry AccountEntry { get; set; } = new()
    {
        Avatar = "../../../Public/Assets/user.png", Tag = "no-login", Username = MainLang.NoLogin
    };
}