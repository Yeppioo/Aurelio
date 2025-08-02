using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Views.Main.Pages;
using Aurelio.Views.Main.Pages.Viewers.Terminal;

namespace Aurelio.Public.Module.App.Services;

public class PageNav
{
    public static void Main()
    {
        UiProperty.NavPages.Add(new NavPageEntry(NewTabPage.StaticPageInfo, NewTabPage.Create));
        UiProperty.NavPages.Add(new NavPageEntry(SettingTabPage.StaticPageInfo, SettingTabPage.Create));
        UiProperty.NavPages.Add(new NavPageEntry(PluginNugetFetcher.StaticPageInfo , PluginNugetFetcher.Create));
        UiProperty.NavPages.Add(new NavPageEntry(TaskCenter.StaticPageInfo , TaskCenter.Create));
    }
}