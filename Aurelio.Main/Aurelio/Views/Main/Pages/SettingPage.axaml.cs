using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Aurelio.Views.Main.SettingPages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ursa.Controls;

namespace Aurelio.Views.Main.Pages;

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
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0,40,0,0), (0,1));
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
    }

    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public SelectionListItem? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public LaunchPage LaunchPage { get; } = new();

    public void OnClose()
    {
    }

    public Border RootElement { get; set; }
}