using System;
using System.Collections.Generic;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App.Init.Config;
using Aurelio.Public.Module.Ui;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages.Functions.CharacterMapping;
using Avalonia.Controls;
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
        page.HostContent = page as UserControl;
        page.HostTab = new TabEntry(page.GetPageInfo().title, page.HostContent!, page.GetPageInfo().icon)
        {
            OnClose = page.GetPageInfo().OnClose
        };
        w.ViewModel.Tabs.Add(page.HostTab);
        w.ViewModel.SelectedItem = page.HostTab;
        App.UiRoot.NewTabButton.Flyout.Hide();
    }
}