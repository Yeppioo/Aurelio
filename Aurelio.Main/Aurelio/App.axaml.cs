using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Aurelio.Public.Const;
using Aurelio.Public.Module.App.Init;
using Avalonia.Markup.Xaml;
using Aurelio.Views.Main;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace Aurelio;

public partial class App : Application
{
    public delegate void UiLoadedEventHandler(MainWindow ui);

    public static MainWindow UiRoot => (Current!.ApplicationLifetime 
        as IClassicDesktopStyleApplicationLifetime).MainWindow as MainWindow;
    public static TopLevel TopLevel => TopLevel.GetTopLevel(UiRoot);
    public static event UiLoadedEventHandler UiLoaded;
    private bool _fl = true;

    public override void Initialize()
    {
        FluentAvalonia.Core.FAUISettings.SetAnimationsEnabledAtAppLevel(false);
        BeforeLoadXaml.Main();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            var win = new MainWindow();
            desktop.MainWindow = win;
            UiProperty.Notification = new Ursa.Controls.WindowNotificationManager(TopLevel.GetTopLevel(win));
            UiProperty.Toast = new Ursa.Controls.WindowToastManager(TopLevel.GetTopLevel(win));
            win.Loaded += (_, _) =>
            {
                if(!_fl) return;
                AfterUiLoaded.Main();
                UiLoaded?.Invoke(win);
                _fl = false;
            };
            UiProperty.Notification.Position = NotificationPosition.BottomRight;
            UiProperty.Toast.MaxItems = 2;
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}