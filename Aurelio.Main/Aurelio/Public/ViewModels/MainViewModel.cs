using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Views.Main.Pages;

namespace Aurelio.Public.ViewModels;

public class MainViewModel : ViewModelBase
{
    private bool _isTabMaskVisible;

    private TabEntry? _selectedTab;
    private Vector _tabScrollOffset;

    public MainViewModel()
    {
        (IAurelioTabPage page, string? tag) obj = Data.SettingEntry.LaunchPage switch
        {
            Setting.LaunchPage.MinecraftInstance => (new MinecraftInstancesTabPage(), "minecraftInstances"),
            Setting.LaunchPage.Setting => (new SettingTabPage(), "setting"),
            _ => (new NewTabPage(), null)
        };
        Tabs.Add(new TabEntry(obj.page) { Tag = obj.tag });
        SelectedTab = Tabs[0];
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(SelectedTab) || SelectedTab == null) return;
            SelectedTab.Content.RootElement.IsVisible = false;
            SelectedTab.Content.InAnimator.Animate();
        };
    }

    public ObservableCollection<TabEntry> Tabs { get; set; } = [];
    public DebugTabPage DebugTabPage { get; set; } = new();


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

    public void CreateTab(TabEntry tab)
    {
        Tabs.Add(tab);
        SelectedTab = tab;
    }
}