using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entries;
using ReactiveUI;
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
    public static WindowNotificationManager Notification { get; set; }
    public static WindowToastManager Toast { get; set; }
}