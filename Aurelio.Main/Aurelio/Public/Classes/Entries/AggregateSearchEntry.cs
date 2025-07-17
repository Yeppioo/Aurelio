using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.Ui;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class AggregateSearchEntry : ReactiveObject
{
    [Reactive] public AggregateSearchEntryType Type { get; set; }
    [Reactive] public StreamGeometry Icon { get; set; }
    [Reactive] public int Order { get; set; }
    [Reactive] public string Title { get; set; }
    [Reactive] public string Summary { get; set; }
    [Reactive] public object OriginObject { get; set; }

    public AggregateSearchEntry(RecordMinecraftEntry entry)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Title = $"{entry.ParentMinecraftFolder.Name}/{entry.Id}";
            Summary = entry.ShortDescription;
            OriginObject = entry;
            Type = AggregateSearchEntryType.MinecraftInstance;
            Icon = StreamGeometry.Parse(
                "M32 32C32 14.3 46.3 0 64 0L320 0c17.7 0 32 14.3 32 32s-14.3 32-32 32l-29.5 0 11.4 148.2c36.7 19.9 65.7 53.2 79.5 94.7l1 3c3.3 9.8 1.6 20.5-4.4 28.8s-15.7 13.3-26 13.3L32 352c-10.3 0-19.9-4.9-26-13.3s-7.7-19.1-4.4-28.8l1-3c13.8-41.5 42.8-74.8 79.5-94.7L93.5 64 64 64C46.3 64 32 49.7 32 32zM160 384l64 0 0 96c0 17.7-14.3 32-32 32s-32-14.3-32-32l0-96z");
            Order = 100;
        });
    }


    public AggregateSearchEntry(RecordMinecraftAccount entry)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Title = $"{entry.AccountType}/{entry.Name}";
            Summary = $"{entry.FormatLastUsedTime} {entry.UUID}";
            OriginObject = entry;
            Type = AggregateSearchEntryType.MinecraftAccount;
            Icon = StreamGeometry.Parse(
                "M64 32C28.7 32 0 60.7 0 96L0 416c0 35.3 28.7 64 64 64l448 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64L64 32zm80 256l64 0c44.2 0 80 35.8 80 80c0 8.8-7.2 16-16 16L80 384c-8.8 0-16-7.2-16-16c0-44.2 35.8-80 80-80zm-32-96a64 64 0 1 1 128 0 64 64 0 1 1 -128 0zm256-32l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16z");
            Order = 200;
        });
    }
}