using System.Linq;
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
        Logger.Info("开始初始化应用程序");
        BeforeLoadXaml.Main();
        AvaloniaXamlLoader.Load(this);
        Logger.Info("应用程序初始化完成");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Logger.Info("框架初始化完成");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
#if DEBUG
            Logger.Debug("开发模式：附加开发工具");
            this.AttachDevTools();
#endif
            DisableAvaloniaDataAnnotationValidation();

#if RELEASE
            Logger.Info("注册全局异常处理");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
#else
            Dispatcher.UIThread.UnhandledException += (_, e) =>
            {
                Logger.Fatal($"UI线程未处理异常: {e.Exception}");
                throw e.Exception;
            };
#endif

            var win = new MainWindow();
            desktop.MainWindow = win;
            UiProperty.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(win));
            UiProperty.Toast = new WindowToastManager(TopLevel.GetTopLevel(win));
            win.Loaded += (_, _) =>
            {
                if (!_fl) return;
                Logger.Info("UI加载完成，执行后续初始化");
                AfterUiLoaded.Main();
                UiLoaded?.Invoke(win);
                _fl = false;
            };
            UiProperty.Notification.Position = NotificationPosition.BottomRight;
            UiProperty.Toast.MaxItems = 2;
            Logger.Info("UI配置完成");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Fatal($"UI线程未处理异常: {e.Exception}");
        try
        {
            var win = new CrashWindow(e.Exception.ToString());
            win.Show();
        }
        catch (Exception ex)
        {
            Logger.Fatal($"显示崩溃窗口失败: {ex}");
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.Fatal($"应用域未处理异常: {e}");
        try
        {
            var win = new CrashWindow(e.ToString() ?? "Unhandled Exception");
            win.Show();
        }
        catch (Exception ex)
        {
            Logger.Fatal($"显示崩溃窗口失败: {ex}");
        }
    }


    private void DisableAvaloniaDataAnnotationValidation()
    {
        Logger.Debug("禁用Avalonia数据注解验证");
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}