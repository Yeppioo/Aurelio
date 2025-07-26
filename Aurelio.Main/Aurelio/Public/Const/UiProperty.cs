using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;

namespace Aurelio.Public.Const;

public class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;

    // 收藏夹功能现在使用独立的布尔属性，不再需要特殊标签
    // 保留 BuiltInTags 集合以备将来可能的系统标签使用
    static UiProperty()
    {
        // 目前没有内置标签，但保留集合结构
    }

    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }
    
    public static ObservableCollection<NotificationEntry> Notifications { get; } = [];
    public static ObservableCollection<string> BuiltInTags { get; } = [];
    public static WindowNotificationManager Notification => ActiveWindow.Notification;
    public static WindowToastManager Toast => ActiveWindow.Toast;
    public static IAurelioWindow ActiveWindow => (Application.Current!.ApplicationLifetime as
        IClassicDesktopStyleApplicationLifetime).Windows.FirstOrDefault
        (x => x.IsActive) as IAurelioWindow ?? App.UiRoot;
}