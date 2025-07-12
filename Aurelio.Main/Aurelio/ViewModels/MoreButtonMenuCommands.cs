using System.Linq;
using Aurelio.Public.Classes.Entries;
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

    public void DebugTab()
    {
        var existingTab = App.UiRoot.Tabs.FirstOrDefault(x => x.Tag == "debug");

        if (existingTab == null)
        {
            var newTab = new TabEntry(App.UiRoot.ViewModel.DebugTabPage)
            {
                Tag = "debug"
            };
            App.UiRoot.Tabs.Add(newTab);
            App.UiRoot.ViewModel.SelectedTab = newTab;
        }
        else
        {
            if (App.UiRoot.SelectedTab == existingTab)
            {
                _ = App.UiRoot.ViewModel.SettingTabPage.Animate();
                return;
            }

            App.UiRoot.ViewModel.SelectedTab = existingTab;
        }
    }
}