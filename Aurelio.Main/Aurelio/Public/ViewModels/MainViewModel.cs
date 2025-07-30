using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Views.Main.Pages;

namespace Aurelio.Public.ViewModels;

public class MainViewModel : ViewModelBase
{
    private bool _isTabMaskVisible;

    private TabEntry? _selectedTab;
    private Vector _tabScrollOffset;

    public MainViewModel()
    {
        InitEvents.AfterUiLoaded += (_, _) =>
        {
            UiProperty.LaunchPages.Add(new LaunchPageEntry
            {
                Id = "NewTab",
                Header = MainLang.NewTab,
                Page = new NewTabPage()
            });
            UiProperty.LaunchPages.Add(new LaunchPageEntry
            {
                Id = "Setting",
                Header = MainLang.Setting,
                Tag = "setting",
                Page = new SettingTabPage()
            });
            var page = UiProperty.LaunchPages.FirstOrDefault(x =>
                x.Id == Data.SettingEntry.LaunchPage.Id) ?? UiProperty.LaunchPages[0];
            Tabs.Add(new TabEntry(page.Page) { Tag = page.Tag });
            SelectedTab = Tabs[0];
        };
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