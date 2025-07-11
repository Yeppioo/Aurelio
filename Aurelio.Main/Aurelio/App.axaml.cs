using System.Diagnostics;
using System.Linq;
using Aurelio.Public.Module.App.Init;
using Aurelio.Views;
using Aurelio.Views.Main;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Threading;
using FluentAvalonia.Core;
using Ursa.Controls;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Aurelio;

public class App : Application
{
    public delegate void UiLoadedEventHandler(MainWindow ui);

    private bool _fl = true;

    public static MainWindow? UiRoot => (Current!.ApplicationLifetime
        as IClassicDesktopStyleApplicationLifetime).MainWindow as MainWindow;

    public static TopLevel TopLevel => TopLevel.GetTopLevel(UiRoot);
    public static event UiLoadedEventHandler UiLoaded;

    public override void Initialize()
    {
        FAUISettings.SetAnimationsEnabledAtAppLevel(false);
        BeforeLoadXaml.Main();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
#if DEBUG
            this.AttachDevTools();
#endif
            DisableAvaloniaDataAnnotationValidation();

#if RELEASE
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
#endif

            var win = new MainWindow();
            desktop.MainWindow = win;
            UiProperty.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(win));
            UiProperty.Toast = new WindowToastManager(TopLevel.GetTopLevel(win));
            win.Loaded += (_, _) =>
            {
                if (!_fl) return;
                AfterUiLoaded.Main();
                UiLoaded?.Invoke(win);
                _fl = false;
            };
            UiProperty.Notification.Position = NotificationPosition.BottomRight;
            UiProperty.Toast.MaxItems = 2;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Console.WriteLine(e.Exception);
        try
        {
            var win = new CrashWindow(e.Exception.ToString());
            win.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine(e);
        try
        {
            var win = new CrashWindow(e.ToString() ?? "Unhandled Exception");
            win.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}