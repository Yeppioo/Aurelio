using System.Text.RegularExpressions;
using System.Web;
using Aurelio.Plugin.Minecraft.Views.SettingPages;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Ursa.Controls;

namespace Aurelio.Plugin.Minecraft.Views;

public partial class SettingTabPage : PageMixModelBase, IAurelioPage
{
    private bool _fl = true;
    private bool _isAnimating;
    private SelectionListItem _selectedItem;
    public int DefaultNav = 0;

    public SettingTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
        AppEvents.AppDragDrop += (o, e) =>
        {
            if (!e.Data.Contains(DataFormats.Text)) return;
            var text = e.Data.GetText();
            if (!text.Trim().StartsWith("authlib-injector:")) return;
            var match = MyRegex().Match(HttpUtility.UrlDecode(text.Trim()));
            if (!match.Success) return;
            var url = match.Value;
            _ = Operate.Account.YggdrasilLogin(this, server1: url);
        };
    }

    public SelectionListItem? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public LaunchPage LaunchPage { get; } = new();
    public AccountPage AccountPage { get; } = new();
    public PageLoadingAnimator InAnimator { get; set; }

    public void OnClose()
    {
    }

    public Control RootElement { get; set; }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            DefaultNav = 0;
            SelectedItem = Nav.Items[DefaultNav] as SelectionListItem;
            _fl = false;
        };
        PropertyChanged += (s, e) =>
        {
            if (SelectedItem == null || e.PropertyName != nameof(SelectedItem)) return;
            if (SelectedItem.Tag is not IAurelioPage page) return;
            page.RootElement.IsVisible = false;
            page.InAnimator.Animate();
        };
    }

    public async Task Animate()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        InAnimator.Animate();
        await Task.Delay(500);
        _isAnimating = false;
    }
    
    [GeneratedRegex("https?://[^\\s:]+")]
    private static partial Regex MyRegex();
}