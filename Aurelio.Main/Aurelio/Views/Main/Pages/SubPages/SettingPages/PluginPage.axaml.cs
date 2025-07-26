using Aurelio.Plugin.Base;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Aurelio.Views.Main.Pages.SubPages.SettingPages;

public partial class PluginPage : PageMixModelBase , IAurelioPage
{
    public PluginPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var root = this.GetVisualRoot();
        if(((Border)sender).Tag is not IPlugin tag) return;
        
        if (root is TabWindow window)
        {
            window.TogglePage(null, new PluginInfo(tag));
            return;
        }
        App.UiRoot.TogglePage(null, new PluginInfo(tag));
    }
}