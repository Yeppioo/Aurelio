using System.Windows.Input;
using Aurelio.Public.Enum;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;

namespace Aurelio.ViewModels;

public partial class MoreButtonMenuCommands
{
    public void NewTab()
    {
        // App.UiRoot.NewTabButton.Flyout.ShowAt(App.UiRoot.FlyoutPoint);
    }

    public void CloseCurrentTab()
    {
        App.UiRoot.ViewModel.SelectedTab?.Close();
    }

    public void ToggleTheme()
    {
        Public.Module.Ui.Setter.ToggleTheme
        (Application.Current.ActualThemeVariant == ThemeVariant.Dark
            ? Setting.Theme.Light : Setting.Theme.Dark);
    }
}