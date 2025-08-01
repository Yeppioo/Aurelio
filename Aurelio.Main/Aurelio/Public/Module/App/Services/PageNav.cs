using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Views.Main.Pages;
using Aurelio.Views.Main.Pages.Viewers.Terminal;

namespace Aurelio.Public.Module.App.Services;

public class PageNav
{
    public static void Main()
    {
        Data.NavPages.Add(new NavPageEntry(NewTabPage.StaticPageInfo, NewTabPage.Create));
        Data.NavPages.Add(new NavPageEntry(SettingTabPage.StaticPageInfo, SettingTabPage.Create));
        Data.NavPages.Add(new NavPageEntry(PluginNugetFetcher.StaticPageInfo , PluginNugetFetcher.Create));
        Data.NavPages.Add(new NavPageEntry(TaskCenter.StaticPageInfo , TaskCenter.Create));
    }
}