using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Langs;
using Aurelio.Views.Main.Pages;
using Aurelio.Views.Main.Pages.Viewers;
using Aurelio.Views.Main.Pages.Viewers.Terminal;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;
using ImageViewer = Aurelio.Views.Main.Pages.Viewers.ImageViewer;

namespace Aurelio.Public.Const;

public class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;

    static UiProperty()
    {
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
    
    public static ObservableCollection<LaunchPageEntry> LaunchPages { get; } = [];
    
    public ObservableCollection<NavPageEntry> Viewers { get; set; } =
    [
        new(CodeViewer.StaticPageInfo, CodeViewer.Create),
        new(TerminalViewer.StaticPageInfo, TerminalViewer.Create),
        new(ImageViewer.StaticPageInfo, ImageViewer.Create),
        new(LogViewer.StaticPageInfo, LogViewer.Create),
        new(JsonViewer.StaticPageInfo, JsonViewer.Create),
        new(ZipViewer.StaticPageInfo, ZipViewer.Create)
    ];

    public static ObservableCollection<NavPageEntry> NavPages { get; } = [];
}