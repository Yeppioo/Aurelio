using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages.Template;
using Avalonia.Controls.Notifications;
using Avalonia.VisualTree;
using WindowBase = Aurelio.Views.Main.WindowBase;

namespace Aurelio.Public.Module.Service;

public class AggregateSearch
{
    public static void Execute(AggregateSearchEntry entry, Control sender)
    {
        if (entry.Type == AggregateSearchEntryType.MinecraftAccount)
        {
            var account = entry.OriginObject
                as RecordMinecraftAccount ?? Data.SettingEntry.MinecraftAccounts.FirstOrDefault();
            if (account == null)
            {
                Notice(MainLang.OperateFailed, NotificationType.Error, host: sender.GetVisualRoot() as WindowBase);
                return;
            }

            Data.SettingEntry.UsingMinecraftAccount = account;
            Notice($"{MainLang.Toggled}: {account.Name}", NotificationType.Success,
                host: sender.GetVisualRoot() as WindowBase);
        }
        else if (entry.Type == AggregateSearchEntryType.MinecraftInstance)
        {
            if (entry.OriginObject is not RecordMinecraftEntry instance)
            {
                Notice(MainLang.OperateFailed, NotificationType.Error, host: sender.GetVisualRoot() as WindowBase);
                return;
            }

            var tab = new TabEntry(new MinecraftInstancePage(instance));
            if (sender.GetVisualRoot() is TabWindow window)
                window.CreateTab(tab);
            else
                Aurelio.App.UiRoot.CreateTab(tab);
        }
    }
}