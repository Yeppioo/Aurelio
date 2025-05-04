using System;
using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entity;
using Aurelio.Public.Content;
using Aurelio.Public.Enum;
using Aurelio.Views.Main.Pages.Home;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace Aurelio.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<NavPage> NavPages { get; } = [];
    public ObservableCollection<NavPage> FooterNavPages { get; } = [];
    public ISukiToastManager ToastManager { get; } = new SukiToastManager();
    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();

    private string _searchText;

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterNavPages();
        }
    }

    private ObservableCollection<NavPage> _filteredNavPages;

    public ObservableCollection<NavPage> FilteredNavPages
    {
        get => _filteredNavPages;
        set => SetProperty(ref _filteredNavPages, value);
    }

    public MainViewModel()
    {
        NavPages.Add(new NavPage("home", "主页", new HomePage(), Icons.FromMaterial(MaterialIconKind.Home)));

        FooterNavPages.Add(new NavPage("plugin", "插件", new HomePage(), Icons.FromMaterial(MaterialIconKind.Puzzle)));
        FooterNavPages.Add(new NavPage("settings", "设置", new HomePage(), Icons.FromMaterial(MaterialIconKind.Settings)));
        FooterNavPages.Add(new NavPage("about", "关于", new HomePage(), Icons.FromMaterial(MaterialIconKind.About)));

        FilteredNavPages = NavPages;
    }

    private void FilterNavPages()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredNavPages = NavPages;
            return;
        }

        var filtered = new ObservableCollection<NavPage>();
        foreach (var page in NavPages)
        {
            var filteredPage = FilterNavPage(page);
            if (filteredPage != null)
            {
                if (!page.Header.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                    filteredPage.SubPages.Count > 0)
                {
                    foreach (var subPage in filteredPage.SubPages)
                    {
                        if (subPage.Header.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        {
                            filtered.Add(subPage);
                        }
                        else
                        {
                            AddMatchingSubPages(subPage, filtered);
                        }
                    }
                }
                else
                {
                    filtered.Add(filteredPage);
                }
            }
        }

        FilteredNavPages = filtered;
    }

    private void AddMatchingSubPages(NavPage page, ObservableCollection<NavPage> result)
    {
        foreach (var subPage in page.SubPages)
        {
            if (subPage.Header.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(subPage);
            }
            else if (subPage.SubPages.Count > 0)
            {
                AddMatchingSubPages(subPage, result);
            }
        }
    }

    private NavPage FilterNavPage(NavPage page)
    {
        var filteredSubPages = new ObservableCollection<NavPage>();
        foreach (var subPage in page.SubPages)
        {
            var filteredSubPage = FilterNavPage(subPage);
            if (filteredSubPage != null)
            {
                filteredSubPages.Add(filteredSubPage);
            }
        }

        if (page.Header.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        {
            return page;
        }

        if (filteredSubPages.Count > 0)
        {
            var result = new NavPage(page.Id, page.Header, page.Content, page.Icon);
            result.SubPages = filteredSubPages;
            return result;
        }

        return null;
    }
}