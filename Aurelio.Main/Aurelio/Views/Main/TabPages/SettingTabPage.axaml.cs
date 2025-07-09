using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Aurelio.Views.Main.SubPages.SettingPages;
using Material.Icons;
using Ursa.Controls;

namespace Aurelio.Views.Main.TabPages;

public partial class SettingTabPage : PageMixModelBase, IAurelioTabPage
{
    public PageLoadingAnimator InAnimator { get; set; }
    private SelectionListItem _selectedItem;
    private bool _fl = true;

    public SettingTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            SelectedItem = Nav.Items[0] as SelectionListItem;
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

    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; } = new()
    {
       Icon = Public.Module.Ui.Icon.FromMaterial(MaterialIconKind.Settings),
       Title = MainLang.Setting
    };

    public SelectionListItem? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public LaunchPage LaunchPage { get; } = new();
    public AccountPage AccountPage { get; } = new();
    public PersonalizationPage PersonalizationPage { get; } = new();

    public void OnClose()
    {
    }

    private bool _isAnimating = false;
    public async Task Animate()
    {
        if(_isAnimating) return;
        _isAnimating = true;
        InAnimator.Animate();
        await Task.Delay(500);
        _isAnimating = false;
    }

    public Border RootElement { get; set; }
}