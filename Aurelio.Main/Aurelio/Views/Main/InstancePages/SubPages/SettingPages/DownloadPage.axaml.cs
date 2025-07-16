using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aurelio.Views.Main.InstancePages.SubPages.SettingPages;

public partial class DownloadPage : PageMixModelBase, IAurelioPage
{
    public DownloadPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    public new static Data Data => Data.Instance;
    public PageLoadingAnimator InAnimator { get; set; }
    public Control RootElement { get; set; }

    private void BindingEvent()
    {
    }
}