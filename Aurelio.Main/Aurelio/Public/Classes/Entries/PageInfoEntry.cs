using Avalonia.Media;

namespace Aurelio.Public.Classes.Entries;

public class PageInfoEntry
{
    public string Title { get; init; }
    public StreamGeometry Icon { get; init; }
    public bool CanClose { get; init; } = true;
}