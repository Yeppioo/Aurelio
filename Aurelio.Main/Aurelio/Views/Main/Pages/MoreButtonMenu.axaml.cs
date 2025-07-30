using Aurelio.Public.Module.Plugin.Events;

namespace Aurelio.Views.Main.Pages;

public partial class MoreButtonMenu : UserControl
{
    public MenuFlyout MenuFlyout => (MenuFlyout)MainControl.Flyout;
    public MoreButtonMenu()
    {
        InitializeComponent();
        InitEvents.OnMoreMenuLoaded(this);
    }
}