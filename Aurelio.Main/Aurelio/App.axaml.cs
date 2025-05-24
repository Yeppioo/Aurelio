using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Aurelio.Public.Module.App.Init;
using Avalonia.Markup.Xaml;
using Aurelio.ViewModels;
using Aurelio.Views;
using Aurelio.Views.Main;
using Avalonia.Controls;

namespace Aurelio;

public partial class App : Application
{
    public delegate void UiLoadedEventHandler(MainWindow ui);

    public MainWindow UiRoot { get; private set; } = null;
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
            UiRoot = win;
            desktop.MainWindow = win;
            win.Loaded += (_, _) =>
            {
                if(!_fl) return;
                AfterUiLoaded.Main();
                UiLoaded?.Invoke(win);
                _fl = false;
            };
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