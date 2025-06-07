using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui;
using Aurelio.ViewModels;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Material.Icons;

namespace Aurelio.Views.Main.Pages;

public partial class HomePage : PageMixModelBase, IFunctionPage
{
    private object _lockObj = new object();

    public HomePage()
    {
        InitializeComponent();
        DataContext = this;
        BindingEvent();
        FilterRecentPages();
    }

    private void BindingEvent()
    {
        ListBox.SelectionChanged += (_, _) =>
        {
            if (ListBox.SelectedItem is not RecentPageEntry entry) return;
            ListBox.SelectedItem = null;
            FunctionConfig.OpenRecentPage(entry);
        };
        _filterRecentPages = new Debouncer(FilterRecentPagesAction, 10);
    }

    public TabEntry HostTab { get; set; }
    private string _searchText = string.Empty;
    public ObservableCollection<RecentPageEntry> FilteredRecentOpens { get; set; } = [];

    public PageInfoEntry PageInfo => new()
    {
        Title = $"{MainLang.MainPage}",
        Icon = Icon.FromMaterial(MaterialIconKind.Home)
    };

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetField(ref _searchText, value);
            FilterRecentPages();
        }
    }

    private Debouncer _filterRecentPages;
    public void FilterRecentPages()
    {
        _filterRecentPages.Trigger();
    }
    public void FilterRecentPagesAction()
    {
        FilteredRecentOpens.Clear();
        FilteredRecentOpens.AddRange(UiProperty.RecentOpens.Where(item =>
                item.Title.Replace(" ", "").ToLower().Contains(SearchText.ToLower().Replace(" ", ""),
                    StringComparison.OrdinalIgnoreCase) ||
                item.Summary.Replace(" ", "").ToLower().Contains(SearchText.ToLower().Replace(" ", ""),
                    StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.LastTime));
    }
    
    

    public void OnClose()
    {
    }
}