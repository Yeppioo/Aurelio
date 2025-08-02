using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Views.Main;
using Avalonia.Controls.ApplicationLifetimes;

namespace Aurelio.Public.Module.App.Services;

public class AppExit
{
    public static void Main()
    {
        AppEvents.AppExiting += AppEventsOnAppExiting;
    }

    private static async Task<bool> AppEventsOnAppExiting()
    {
        var requestCloseTab = await RequestCloseTabs();
        return requestCloseTab;
    }

    private static async Task<bool> RequestCloseTabs()
    {
        foreach (var tabEntry in Aurelio.App.UiRoot.Tabs
                     .Where(x => x.Content is IAurelioRequestableClosePage))
        {
            var close = await (tabEntry.Content as IAurelioRequestableClosePage).RequestClose(Aurelio.App.UiRoot);
            if (!close)
            {
                return false;
            }
        }

        var ws = (Application.Current!.ApplicationLifetime as
            IClassicDesktopStyleApplicationLifetime).Windows.OfType<TabWindow>();
        foreach (var tabWindow in ws)
        {
            foreach (var tabEntry in tabWindow.Tabs
                         .Where(x => x.Content is IAurelioRequestableClosePage))
            {
                var close = await (tabEntry.Content as IAurelioRequestableClosePage).RequestClose(Aurelio.App.UiRoot);
                if (!close)
                {
                    return false;
                }
            }
        }
        return true;
    }
}