using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Langs;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;
using WindowBase = Aurelio.Views.Main.WindowBase;

namespace Aurelio.Public.Const;

public class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;
    public static readonly string FavouriteTag = MainLang.Favourite;

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

    public static WindowNotificationManager Notification
    {
        get
        {
            var active = (Application.Current!.ApplicationLifetime as
                IClassicDesktopStyleApplicationLifetime).Windows.FirstOrDefault(x => x.IsActive);
            return (active as WindowBase)?.Notification;
        }
    }

    public static WindowToastManager Toast
    {
        get
        {
            var active = (Application.Current!.ApplicationLifetime as
                IClassicDesktopStyleApplicationLifetime).Windows.FirstOrDefault(x => x.IsActive);
            return (active as WindowBase)?.Toast;
        }
    }
}