using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Langs;
using Aurelio.Views.Main.Pages;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;

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

    /*<ComboBoxItem Content="NewTab" Tag="{Binding Source={x:Static properties:MainLangProvider.Current}, Path=Resources.NewTab}" />
                                    <ComboBoxItem Content="Setting" Tag="{Binding Source={x:Static properties:MainLangProvider.Current}, Path=Resources.Setting}" />*/

    public static ObservableCollection<LaunchPageEntry> LaunchPages { get; } = [];
}