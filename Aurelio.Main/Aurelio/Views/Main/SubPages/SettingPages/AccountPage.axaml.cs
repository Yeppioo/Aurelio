using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;

namespace Aurelio.Views.Main.SubPages.SettingPages;

public partial class AccountPage : PageMixModelBase, IAurelioPage
{
    public AccountPage()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    private void BindingEvent()
    {
        AddAccount.Click += (_, _) => { _ = Public.Module.Op.Account.AddByUi(this); };
        // Open3DView.Click += (_, _) => { Data.SettingEntry.UsingMinecraftAccount.Render3D();};
        DelSelectedAccount.Click += (_, _) => { Public.Module.Op.Account.RemoveSelected(); };
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}