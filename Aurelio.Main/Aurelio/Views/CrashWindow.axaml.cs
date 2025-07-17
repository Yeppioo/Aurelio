using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Ursa.Controls;

namespace Aurelio.Views;

public partial class CrashWindow : UrsaWindow
{
    public CrashWindow(string exception)
    {
        InitializeComponent();
        Setter.UpdateWindowStyle(this);

        Info.Text = exception;
        Copy.Click += async (_, _) =>
        {
            var clipboard = GetTopLevel(this)?.Clipboard;
            await clipboard.SetTextAsync(exception);
        };
        Continue.Click += (_, _) => { Close(); };
        Restart.Click += (_, _) => { AppMethod.RestartApp(); };
        Exit.Click += (_, _) => { Environment.Exit(0); };
        Topmost = true;
        Loaded += (_, _) => { Setter.UpdateWindowStyle(this); };
        Show();
        Activate();
    }

    public CrashWindow()
    {
    }

    public sealed override void Show()
    {
        base.Show();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Data.DesktopType == DesktopType.Linux ||
            Data.DesktopType == DesktopType.FreeBSD ||
            (Data.DesktopType == DesktopType.Windows &&
             Environment.OSVersion.Version.Major < 10))
        {
            IsManagedResizerVisible = true;
            SystemDecorations = SystemDecorations.None;
            Root.CornerRadius = new CornerRadius(0);
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaToDecorationsHint = true;
        }
        else if (Data.DesktopType == DesktopType.MacOs)
        {
            SystemDecorations = SystemDecorations.Full;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
            ExtendClientAreaToDecorationsHint = true;
            TitleRoot.Margin = new Thickness(65, 0, 0, 0);
            TitleBar.IsCloseBtnShow = false;
            TitleBar.IsMinBtnShow = false;
            TitleBar.IsMaxBtnShow = false;
        }
        else
        {
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaToDecorationsHint = true;
        }
        
        if (Data.DesktopType == DesktopType.MacOs)
        {
            PropertyChanged += (_, _) =>
            {
                var platform = TryGetPlatformHandle();
                if (platform is null) return;
                var nsWindow = platform.Handle;
                if (nsWindow == IntPtr.Zero) return;
                try
                {
                    MacOsWindowHandler.RefreshTitleBarButtonPosition(nsWindow);
                    MacOsWindowHandler.HideZoomButton(nsWindow);

                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            };
        }
    }
}