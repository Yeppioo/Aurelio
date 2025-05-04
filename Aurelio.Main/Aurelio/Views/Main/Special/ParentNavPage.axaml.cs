using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entity;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;

namespace Aurelio.Views.Main.Special;

public partial class ParentNavPage : UserControl
{
    private readonly string _id;
    private NavPage _page;
    public ObservableCollection<NavPage> Pages { get; set; } = [];

    public ParentNavPage(string id)
    {
        _id = id;
        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) =>
        {
            RefreshPages();
            _page = App.UiRoot.NavPages.FindById(_id);
            _page.SubPages.CollectionChanged += (_, _) => RefreshPages();
        };
    }

    private void RefreshPages()
    {
        Pages.Clear();
        foreach (var subPage in App.UiRoot.NavPages.FindById(_id).SubPages)
        {
            Pages.Add(subPage);
        }
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var id = ((GlassCard)sender).Tag!.ToString()!;
        var item = App.UiRoot.NavPages.FindById(id);
        if (!_page.IsExpanded)
        {
            _page.IsExpanded = true;
        }
        App.UiRoot.NavMenu.SelectedItem = item;
    }
}