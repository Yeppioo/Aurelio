using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
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
    public HomePage()
    {
        InitializeComponent();
        DataContext = this;
        FilterRecentOpens();
    }

    public TabEntry HostTab { get; set; }
    private string _searchText = string.Empty;
    public ObservableCollection<RecentOpenEntry> FilteredRecentOpens { get; set; } = [];

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
            FilterRecentOpens();
        }
    }

    public void FilterRecentOpens()
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