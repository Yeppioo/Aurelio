using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages;
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
        if (entry.Type == AggregateSearchEntryType.MinecraftAccount)
        {
            var account = entry.OriginObject
                as RecordMinecraftAccount ?? Data.SettingEntry.MinecraftAccounts.FirstOrDefault();
            if (account == null)
            {
                Notice(MainLang.OperateFailed, NotificationType.Error, host: visualRoot as IAurelioWindow);
                return;
            }

            Data.SettingEntry.UsingMinecraftAccount = account;
            Notice($"{MainLang.Toggled}: {account.Name}", NotificationType.Success,
                host: visualRoot as IAurelioWindow);
        }
        else if (entry.Type == AggregateSearchEntryType.MinecraftInstance)
        {
            if (entry.OriginObject is not RecordMinecraftEntry instance)
            {
                Notice(MainLang.OperateFailed, NotificationType.Error, host: visualRoot as IAurelioWindow);
                return;
            }

            var tab = new TabEntry(new MinecraftInstancePage(instance));
            if (visualRoot is TabWindow window)
                window.CreateTab(tab);
            else
                Aurelio.App.UiRoot.CreateTab(tab);
        }
        else if (entry.Type == AggregateSearchEntryType.AurelioTabPage)
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
        else if (entry.Type == AggregateSearchEntryType.SystemFile)
        {
            // SystemFile entries are handled directly by NewTabPage
            // No action needed here as the navigation is handled in the SelectionChanged event
            return;
        }
    }
}