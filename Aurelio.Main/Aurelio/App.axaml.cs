using System.Linq;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App.Init;
using Aurelio.Public.Module.IO;
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
        Logger.Info(MainLang.StartInitAppTip);
        BeforeLoadXaml.Main();
        AvaloniaXamlLoader.Load(this);
        Logger.Info(MainLang.AppInitCompleteTip);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Logger.Info(MainLang.FrameworkInitCompleteTip);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
#if DEBUG
            Logger.Debug(MainLang.DevModeAttachDevToolsTip);
            this.AttachDevTools();
#endif
            DisableAvaloniaDataAnnotationValidation();

#if RELEASE
            Logger.Info(MainLang.RegisterGlobalExceptionHandlerTip);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
#else
            Dispatcher.UIThread.UnhandledException += (_, e) =>
            {
                Logger.Fatal($"{MainLang.UIThreadUnhandledExceptionTip}: {e.Exception}");
                throw e.Exception;
            };
#endif
            var win = new MainWindow();
            desktop.MainWindow = win;
            win.Loaded += (_, _) =>
            {
                if (!_fl) return;
                Logger.Info(MainLang.UILoadCompleteTip);
                AfterUiLoaded.Main();
                UiLoaded?.Invoke(win);
                _fl = false;
            };
            Logger.Info(MainLang.UIConfigCompleteTip);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Fatal($"{MainLang.UIThreadUnhandledExceptionTip}: {e.Exception}");
        try
        {
            var win = new CrashWindow(e.Exception.ToString());
            win.Show();
        }
        catch (Exception ex)
        {
            Logger.Fatal($"{MainLang.ShowCrashWindowFailTip}: {ex}");
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.Fatal($"{MainLang.AppDomainUnhandledExceptionTip}: {e}");
        try
        {
            var win = new CrashWindow(e.ToString() ?? "Unhandled Exception");
            win.Show();
        }
        catch (Exception ex)
        {
            Logger.Fatal($"{MainLang.ShowCrashWindowFailTip}: {ex}");
        }
    }


    private void DisableAvaloniaDataAnnotationValidation()
    {
        Logger.Debug(MainLang.DisableAvaloniaDataValidationTip);
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}