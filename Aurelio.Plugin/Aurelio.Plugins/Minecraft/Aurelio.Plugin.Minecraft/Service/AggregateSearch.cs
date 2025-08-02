using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Views;
using Aurelio.Plugin.Minecraft.Views.FetcherPages.Vanilla;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Pages;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;

namespace Aurelio.Plugin.Minecraft.Service;

public class AggregateSearch
{
    public static void Main()
    {
        AggregateSearchEvents.UpdateAggregateSearchEntries += (_, _) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Data.AggregateSearchEntries.Add(new AggregateSearchEntry(new MinecraftInstancesTabPage(),
                    MainLang.MinecraftInstance));
                Data.AggregateSearchEntries.Add(new AggregateSearchEntry(new VersionSelector(),
                    MainLang.MinecraftInstance));
                Data.AggregateSearchEntries.AddRange(MinecraftPluginData.AllMinecraftInstances.Select(Create));
                if (Data.SettingEntry != null)
                    Data.AggregateSearchEntries.AddRange(
                        MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Select(Create));
            });
        };
        
        AggregateSearchEvents.ExecuteAggregateSearch += (sender, entry) =>
        {
            if (entry.Type is "Plugin.Minecraft.MinecraftInstance")
            {
                var tab = new TabEntry(new MinecraftInstancePage((entry.OriginObject as RecordMinecraftEntry)!));
                if ((sender as Control)!.GetVisualRoot() is TabWindow window)
                {
                    window.CreateTab(tab);
                    return;
                }

                App.UiRoot.CreateTab(tab);
                if (sender is NewTabPage page)
                {
                    page.HostTab.Close(page.GetVisualRoot());
                }

                return;
            }

            if (entry.Type is "Plugin.Minecraft.MinecraftAccount")
            {
                MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount =
                    entry.OriginObject as RecordMinecraftAccount;
                Notice($"{MainLang.SwitchedTo}: {(entry.OriginObject as RecordMinecraftAccount).Name}",
                    NotificationType.Success);
            }
        };
    }

    public static AggregateSearchEntry Create(RecordMinecraftEntry entry)
    {
        return new AggregateSearchEntry
        {
            Title = $"{entry.ParentMinecraftFolder.Name}/{entry.Id}",
            Summary = entry.ShortDescription,
            OriginObject = entry,
            Type = "Plugin.Minecraft.MinecraftInstance",
            Icon = StreamGeometry.Parse(
                "M32 32C32 14.3 46.3 0 64 0L320 0c17.7 0 32 14.3 32 32s-14.3 32-32 32l-29.5 0 11.4 148.2c36.7 19.9 65.7 53.2 79.5 94.7l1 3c3.3 9.8 1.6 20.5-4.4 28.8s-15.7 13.3-26 13.3L32 352c-10.3 0-19.9-4.9-26-13.3s-7.7-19.1-4.4-28.8l1-3c13.8-41.5 42.8-74.8 79.5-94.7L93.5 64 64 64C46.3 64 32 49.7 32 32zM160 384l64 0 0 96c0 17.7-14.3 32-32 32s-32-14.3-32-32l0-96z"),
            Order = 100
        };
    }

    public static AggregateSearchEntry Create(RecordMinecraftAccount entry)
    {
        return new AggregateSearchEntry
        {
            Title = $"{entry.AccountType}/{entry.Name}",
            Summary = $"{entry.FormatLastUsedTime} {entry.UUID}",
            OriginObject = entry,
            Type = "Plugin.Minecraft.MinecraftAccount",
            Icon = StreamGeometry.Parse(
                "M64 32C28.7 32 0 60.7 0 96L0 416c0 35.3 28.7 64 64 64l448 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64L64 32zm80 256l64 0c44.2 0 80 35.8 80 80c0 8.8-7.2 16-16 16L80 384c-8.8 0-16-7.2-16-16c0-44.2 35.8-80 80-80zm-32-96a64 64 0 1 1 128 0 64 64 0 1 1 -128 0zm256-32l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16z"
            ),
            Order = 200
        };
    }
}