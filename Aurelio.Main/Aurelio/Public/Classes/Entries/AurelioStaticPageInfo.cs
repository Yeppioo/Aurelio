using Aurelio.Public.Classes.Interfaces;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Entries;

public class AurelioStaticPageInfo
{
    public StreamGeometry Icon { get; set; }
    public string Title { get; set; }
    public bool NeedPath { get; set; }
    public bool MustPath { get; set; }
    public bool AutoCreate { get; set; }
}