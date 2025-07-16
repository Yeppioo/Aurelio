using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;

namespace Aurelio.Views.Main.InstancePages.SubPages.SettingPages;

public partial class AurelioPage : PageMixModelBase , IAurelioPage
{
    public AurelioPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}