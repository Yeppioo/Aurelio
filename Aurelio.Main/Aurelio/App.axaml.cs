using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Aurelio.Public.Module.Services;
using Avalonia.Markup.Xaml;
using Aurelio.ViewModels;
using Aurelio.Views;
using Avalonia.Media;
using SukiUI;
using SukiUI.Enums;
using SukiUI.Models;
using MainWindow = Aurelio.Views.Main.MainWindow;

namespace Aurelio;

public partial class App : Application
{
    public static MainWindow UiRoot { get; private set; }
    public override void Initialize()
    {
        Public.Module.App.Init.Main.Init();
        AvaloniaXamlLoader.Load(this);
        FluentAvalonia.Core.FAUISettings.SetAnimationsEnabledAtAppLevel(false);
        Theme.ChangeThemeColor(Color.Parse("#0AFF81"), Color.Parse("#7B80FF"));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            var win = new MainWindow();
            UiRoot = win;
            desktop.MainWindow = win;
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