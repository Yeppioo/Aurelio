using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Views.Main;
using Avalonia.Controls.Notifications;
using Avalonia.Rendering;
using Avalonia.VisualTree;

namespace Aurelio.Public.Module.Service;

public class AggregateSearch
{
    public static void Execute(AggregateSearchEntry entry, Control sender)
    {
        IRenderRoot? renderRoot = null;
        Execute(entry, sender, ref renderRoot);
    }

    public static void Execute(AggregateSearchEntry entry, Control sender, ref IRenderRoot? renderRoot)
    {
        var visualRoot = sender.GetVisualRoot();
        renderRoot = visualRoot;
        if (entry.Type is not AggregateSearchEntryType t)
        {
            AppEvents.OnExecuteAggregateSearch(entry, sender);
            return;
        }
        if (t == AggregateSearchEntryType.AurelioTabPage)
        {
            if (entry.OriginObject is not IAurelioTabPage page)
            {
                Notice(MainLang.OperateFailed, NotificationType.Error, host: visualRoot as IAurelioWindow);
                return;
            }

            if (visualRoot is TabWindow window)
            {
                window.TogglePage(entry.Tag, page);
                return;
            }
            Aurelio.App.UiRoot.TogglePage(entry.Tag, page);
        }
        else if (t == AggregateSearchEntryType.SystemFile)
        {
            // SystemFile entries are handled directly by NewTabPage
            // No action needed here as the navigation is handled in the SelectionChanged event
            return;
        }
    }
}