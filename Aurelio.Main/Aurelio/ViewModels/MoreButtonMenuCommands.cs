using Aurelio.Public.Classes.Enum;
using Avalonia.Styling;
using Setter = Aurelio.Public.Module.Ui.Setter;

namespace Aurelio.ViewModels;

public class MoreButtonMenuCommands
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
        Setter.ToggleTheme
        (Application.Current.ActualThemeVariant == ThemeVariant.Dark
            ? Setting.Theme.Light
            : Setting.Theme.Dark);
    }
}