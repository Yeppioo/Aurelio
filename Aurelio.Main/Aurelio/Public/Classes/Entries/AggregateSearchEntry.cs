using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
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
    [Reactive] public string Tag { get; set; }
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

    public AggregateSearchEntry(IAurelioTabPage entry, string? tag)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Title = $"{entry.PageInfo.Title}";
            Summary = $"{MainLang.OpenOrTogglePage}";
            OriginObject = entry;
            Type = AggregateSearchEntryType.AurelioTabPage;
            Tag = tag;
            Icon = StreamGeometry.Parse(
                "F1 M 12.670898 5.825195 L 15 6.796875 L 15.97168 9.125977 C 16.025391 9.228516 16.132812 9.296875 16.25 9.296875 C 16.367188 9.296875 16.474609 9.228516 16.52832 9.125977 L 17.5 6.796875 L 19.829102 5.825195 C 19.931641 5.771484 20 5.664062 20 5.546875 C 20 5.429688 19.931641 5.322266 19.829102 5.268555 L 17.5 4.296875 L 16.52832 1.967773 C 16.474609 1.865234 16.367188 1.796875 16.25 1.796875 C 16.132812 1.796875 16.025391 1.865234 15.97168 1.967773 L 15 4.296875 L 12.670898 5.268555 C 12.568359 5.322266 12.5 5.429688 12.5 5.546875 C 12.5 5.664062 12.568359 5.771484 12.670898 5.825195 Z M 19.829102 17.768555 L 17.5 16.796875 L 16.52832 14.467773 C 16.474609 14.365234 16.367188 14.296875 16.25 14.296875 C 16.132812 14.296875 16.025391 14.365234 15.97168 14.467773 L 15 16.796875 L 12.670898 17.768555 C 12.568359 17.822266 12.5 17.929688 12.5 18.046875 C 12.5 18.164062 12.568359 18.271484 12.670898 18.325195 L 15 19.296875 L 15.97168 21.625977 C 16.025391 21.728516 16.132812 21.796875 16.25 21.796875 C 16.367188 21.796875 16.474609 21.728516 16.52832 21.625977 L 17.5 19.296875 L 19.829102 18.325195 C 19.931641 18.271484 20 18.164062 20 18.046875 C 20 17.929688 19.931641 17.822266 19.829102 17.768555 Z M 15 11.782227 C 15 11.547852 14.868164 11.328125 14.65332 11.220703 L 10.258789 9.018555 L 8.056641 4.614258 C 7.84668 4.189453 7.15332 4.189453 6.943359 4.614258 L 4.741211 9.018555 L 0.34668 11.220703 C 0.131836 11.328125 0 11.547852 0 11.782227 C 0 12.021484 0.131836 12.236328 0.34668 12.34375 L 4.741211 14.545898 L 6.943359 18.950195 C 7.045898 19.160156 7.265587 19.296875 7.5 19.296875 C 7.734337 19.296875 7.954102 19.160156 8.056641 18.950195 L 10.258789 14.545898 L 14.65332 12.34375 C 14.868164 12.236328 15 12.021484 15 11.782227 Z ");
            Order = 300;
        });
    }
}