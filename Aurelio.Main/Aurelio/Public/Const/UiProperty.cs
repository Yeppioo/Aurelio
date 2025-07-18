using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Value;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;

namespace Aurelio.Public.Const;

public class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;

    // 使用固定标识符，不依赖语言
    public static readonly string FavouriteTag = "__FAVOURITE__";

    // 用于显示的本地化收藏夹名称
    public static string FavouriteDisplayName => MainLang.Favourite;

    static UiProperty()
    {
        BuiltInTags.Add(FavouriteTag);
    }

    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }

    [Reactive] public bool IsRender3D { get; set; }

    public static ObservableCollection<NotificationEntry> Notifications { get; } = [];
    public static ObservableCollection<string> AllMinecraftTags { get; } = [];
    public static ObservableCollection<string> BuiltInTags { get; } = [];
    public static WindowNotificationManager Notification => ActiveWindow.Notification;
    public static WindowToastManager Toast => ActiveWindow.Toast;
    public static Bitmap WindowBackGroundImg => Converter.Base64ToBitmap(Data.SettingEntry.BackGroundImgData);

    public static IAurelioWindow ActiveWindow => (Application.Current!.ApplicationLifetime as
        IClassicDesktopStyleApplicationLifetime).Windows.FirstOrDefault
        (x => x.IsActive) as IAurelioWindow ?? App.UiRoot;
}