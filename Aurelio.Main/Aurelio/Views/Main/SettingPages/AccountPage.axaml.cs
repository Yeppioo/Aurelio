using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aurelio.Views.Main.SettingPages;

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
    }

    public Border RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
}