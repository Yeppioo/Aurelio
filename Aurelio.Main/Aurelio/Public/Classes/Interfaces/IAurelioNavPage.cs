using Aurelio.Public.Classes.Entries;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioNavPage
{
    static AurelioStaticPageInfo StaticPageInfo { get; }
    static abstract IAurelioNavPage Create((object sender, object? param)t);
}