using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
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
        new(canClose: false, title: MainLang.MainPage, icon: Icons.Home,
            content: PageInstance.HomeTabPage),
    ];
    

    private TabEntry? _selectedTab;
    private Vector _tabScrollOffset;
    private bool _isTabMaskVisible;
    
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

    public TabEntry? SelectedTab
    {
        get => _selectedTab;
        set => SetField(ref _selectedTab, value);
    }

    public MainViewModel()
    {
        SelectedTab = Tabs[0];
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(SelectedTab) || SelectedTab == null) return;
            SelectedTab.Content.RootElement.IsVisible = false;
            SelectedTab.Content.InAnimator.Animate();
        };
    }
}