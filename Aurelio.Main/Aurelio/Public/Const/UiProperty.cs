using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Module;
using ReactiveUI;
using Ursa.Controls;

namespace Aurelio.Public.Const;

public class UiProperty : ReactiveObject
{
    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }

    private static UiProperty? _instance;
    public static ObservableCollection<NotificationEntry> NotificationCards { get; } = [];
    public static WindowNotificationManager Notification { get; set; }
    public static WindowToastManager Toast { get; set; }
    public static ObservableCollection<RecentPageEntry> RecentOpens { get; } = [];

    public UiProperty()
    {
    }
}