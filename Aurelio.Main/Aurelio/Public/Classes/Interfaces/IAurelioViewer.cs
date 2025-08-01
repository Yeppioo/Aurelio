using Aurelio.Public.Classes.Entries;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioViewer
{
    static AurelioViewerInfo ViewerInfo { get; }
    static abstract IAurelioViewer Create(string path);
}

