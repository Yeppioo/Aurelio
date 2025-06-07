using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.App.Init.Config;
using Aurelio.Public.Module.Ui;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages;
using Aurelio.Views.Main.Pages.Functions.CharacterMapping;
using Avalonia.Controls;
using DynamicData;
using Material.Icons;

namespace Aurelio.ViewModels;

public abstract class FunctionConfig
{
    public static List<NewPageEntry> FunctionItems { get; set; } =
    [
        new(FunctionType.CharacterMapping, MainLang.CharacterMapping, Icons.CharacterAppearance),
    ];

    public static void CreateNewTab(FunctionType type)
    {
        var w = App.UiRoot;
        IFunctionPage page = type switch
        {
            FunctionType.CharacterMapping => new FontSelectionPage(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        var tab = new TabEntry(page.PageInfo.Title, page, page.PageInfo.Icon);
        page.HostTab = tab;
        w.ViewModel.Tabs.Add(tab);
        w.ViewModel.SelectedItem = tab;
        App.UiRoot.NewTabButton.Flyout.Hide();
    }
    
    public static void OpenRecentPage(RecentPageEntry entry)
    {
        var page = entry.FunctionType switch
        {
            FunctionType.CharacterMapping => FontMappingTablePage.RecentOpenHandle(entry),
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null)
        };
        if (page == null) return;
        var w = App.UiRoot;
        var tab = new TabEntry(null,null);
        tab.ReplacePage(page);
        w.ViewModel.Tabs.Add(tab);
        w.ViewModel.SelectedItem = tab;
    }

    public static void AddRecentOpen(RecentPageEntry entry)
    {
        UiProperty.RecentOpens.RemoveMany(UiProperty.RecentOpens.Where(x => Equals(x, entry)));
        UiProperty.RecentOpens.Add(entry);
        (App.UiRoot.ViewModel.Tabs.First().Content as HomePage)?.FilterRecentPages();
        File.WriteAllText(ConfigPath.RecentOpenDataPath, UiProperty.RecentOpens.AsJson());
    }
}