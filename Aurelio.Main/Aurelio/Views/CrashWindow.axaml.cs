using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.App;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Ursa.Controls;

namespace Aurelio.Views;

public partial class CrashWindow : UrsaWindow
{
    public CrashWindow(string exception)
    {
        InitializeComponent();
        Public.Module.Ui.Setter.UpdateWindowStyle(this);
        
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
        Loaded += (_, _) =>
        {
            Public.Module.Ui.Setter.UpdateWindowStyle(this);
        };
        Show();
        Activate();
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
        }

        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaToDecorationsHint = true;
    }
}