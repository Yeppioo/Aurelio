using System;
using System.Collections.Generic;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Types;
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
        using IFunctionPage page = type switch
        {
            FunctionType.CharacterMapping => new FontSelectionPage(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        var tab = new TabEntry(page.GetPageInfo().title, page, page.GetPageInfo().icon);
        page.HostTab = tab;
        w.ViewModel.Tabs.Add(tab);
        w.ViewModel.SelectedItem = tab;
        App.UiRoot.NewTabButton.Flyout.Hide();
    }
}