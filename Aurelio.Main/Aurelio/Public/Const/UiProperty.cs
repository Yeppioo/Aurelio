using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Langs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;

namespace Aurelio.Public.Const;

public class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;

    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }

    public static ObservableCollection<NotificationEntry> NotificationCards { get; } = [];
    public static ObservableCollection<string> AllMinecraftTags { get; } = [];
    public static ObservableCollection<string> BuiltInTags { get; } = [];
    public static readonly string FavouriteTag = MainLang.Favourite;
    public static WindowNotificationManager Notification { get; set; }
    public static WindowToastManager Toast { get; set; }
    
    // 初始化内置标签
    static UiProperty()
    {
        // 添加收藏夹标签到内置标签集合
        BuiltInTags.Add(FavouriteTag);
    }
}