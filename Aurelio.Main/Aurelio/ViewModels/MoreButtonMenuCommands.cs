using System.Windows.Input;
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
}