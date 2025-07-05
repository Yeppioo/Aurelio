using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.ViewModels;
using Aurelio.Views.Main.SettingPages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ursa.Controls;

namespace Aurelio.Views.Main.Pages;

public partial class SettingPage : PageMixModelBase, IAurelioPage
{
    private SelectionListItem _selectedItem;
    private bool _fl = true;
    public SettingPage()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) =>
        {
            if(!_fl) return;
            SelectedItem = Nav.Items[0] as SelectionListItem;
            _fl = false;
        };
    }

    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public SelectionListItem SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }
    
    public LaunchPage LaunchPage { get; } = new();

    public void OnClose()
    {
    }
}