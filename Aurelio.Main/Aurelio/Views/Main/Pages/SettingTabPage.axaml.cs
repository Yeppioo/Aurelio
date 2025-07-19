using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.SubPages.SettingPages;
using Ursa.Controls;

namespace Aurelio.Views.Main.Pages;

public partial class SettingTabPage : PageMixModelBase, IAurelioTabPage
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
        PageInfo = new PageInfoEntry
        {
            Icon = Icons.Setting,
            Title = MainLang.Setting
        };
    }

    public SelectionListItem? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public LaunchPage LaunchPage { get; } = new();
    public AccountPage AccountPage { get; } = new();
    public DownloadPage DownloadPage { get; } = new();
    public AurelioPage AurelioPage { get; } = new();
    public PersonalizationPage PersonalizationPage { get; } = new();
    public PageLoadingAnimator InAnimator { get; set; }

    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; }

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
}