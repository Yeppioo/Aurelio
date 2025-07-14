using System;
using Aurelio.Public.Module.IO;
using Avalonia;
using Avalonia.Dialogs;
using HotAvalonia;

namespace Aurelio.Desktop;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // 初始化日志系统
        Logger.Initialize();
        Logger.Info("应用程序启动");

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseManagedSystemDialogs()
#if DEBUG
            .UseHotReload()
#endif
            .UsePlatformDetect()
            .LogToTrace();
}