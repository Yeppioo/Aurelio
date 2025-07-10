using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;

namespace Aurelio.ViewModels;

public class TabWindowViewModel : ViewModelBase
{
    private bool _isTabMaskVisible;

    private TabEntry? _selectedTab;
    private Vector _tabScrollOffset;

    public TabWindowViewModel()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(SelectedTab) || SelectedTab == null) return;
            SelectedTab.Content.RootElement.IsVisible = false;
            SelectedTab.Content.InAnimator.Animate();
        };
    }

    public ObservableCollection<TabEntry> Tabs { get; set; } = [];

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

    public bool HasTabs => Tabs.Count > 0;

    public void CreateTab(TabEntry tab)
    {
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void AddTab(TabEntry tab)
    {
        Tabs.Add(tab);
        if (SelectedTab == null)
            SelectedTab = tab;
    }

    public void RemoveTab(TabEntry tab)
    {
        if (Tabs.Contains(tab))
        {
            var wasSelected = SelectedTab == tab;
            Tabs.Remove(tab);

            // If the removed tab was selected, select the last remaining tab (or null if no tabs left)
            if (wasSelected) SelectedTab = Tabs.LastOrDefault();

            // Notify that tabs collection changed
            OnPropertyChanged(nameof(HasTabs));
            OnTabsCollectionChanged();
        }
    }

    public event Action? TabsEmptied;

    private void OnTabsCollectionChanged()
    {
        if (!HasTabs) TabsEmptied?.Invoke();
    }
}