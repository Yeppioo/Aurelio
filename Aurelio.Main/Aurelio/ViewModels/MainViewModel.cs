using System;
using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.Views.Main.Pages;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Material.Icons;

namespace Aurelio.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<TabEntry> Tabs { get; set; } =
    [
        new(canClose: false, title: MainLang.MainPage, icon: Icon.FromMaterial(MaterialIconKind.Home),
            content: new HomePage()),
    ];

    public ObservableCollection<NewPageEntry> NewPageItems { get; set; } = [];

    private TabEntry? _selectedItem;
    private Vector _tabScrollOffset;
    private bool _isTabMaskVisible;
    private string _searchFunctionText = string.Empty;

    public string SearchFunctionText
    {
        get => _searchFunctionText;
        set
        {
            SetField(ref _searchFunctionText, value);
            FilterFunction();
        }
    }
    
    public Vector TabScrollOffset
    {
        get => _tabScrollOffset;
        set => SetField(ref _tabScrollOffset, value);
    }

    public bool IsTabMaskVisible
    {
        get => _isTabMaskVisible;
        set => SetField(ref _isTabMaskVisible, value);
    }

    public TabEntry? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public MainViewModel()
    {
        SelectedItem = Tabs[0];
        FunctionConfig.FunctionItems = FunctionConfig.FunctionItems.OrderBy(x => x.Title).ToList();
        FilterFunction();
    }

    private void FilterFunction()
    {
        NewPageItems.Clear();
        NewPageItems.AddRange(FunctionConfig.FunctionItems.Where(item =>
                item.Title.ToLower().Contains(SearchFunctionText.ToLower(), StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(x => x.Title).ToList());
    }
}