﻿using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages;
using Avalonia.Styling;
using Setter = Aurelio.Public.Module.Ui.Setter;

namespace Aurelio.Public.ViewModels;

public class MoreButtonMenuCommands
{
    public void NewTab()
    {
        if (UiProperty.ActiveWindow is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new NewTabPage()));
            return;
        }
        App.UiRoot.CreateTab(new TabEntry(new NewTabPage()));
    }

    public void CloseCurrentTab()
    {
        if (UiProperty.ActiveWindow is TabWindow tabWindow)
        {
            tabWindow.ViewModel.SelectedTab?.Close();
            return;
        }
        App.UiRoot.ViewModel.SelectedTab?.Close();
    }

    public void ToggleTheme()
    {
        Data.SettingEntry.Theme = Application.Current.ActualThemeVariant == ThemeVariant.Dark
            ? Setting.Theme.Light
            : Setting.Theme.Dark;
    }

    public void DebugTab()
    {
        App.UiRoot.TogglePage("debug", App.UiRoot.ViewModel.DebugTabPage);
    }

    public void OpenInstancePage(string page)
    {
        switch (page)
        {
            case "minecraftInstances":
                App.UiRoot.TogglePage("minecraftInstances" , new MinecraftInstancesTabPage());
                break;
            case "setting":
                App.UiRoot.TogglePage("setting", new SettingTabPage());
                break;
        }
    }
}