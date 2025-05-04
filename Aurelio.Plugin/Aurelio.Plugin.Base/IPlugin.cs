using Avalonia.Media.Imaging;

namespace Aurelio.Plugin.Base;

public interface IPlugin
{
    string Name { get; }
    string Author { get; }
    string Description { get; }
    string Version { get; }
    Bitmap Icon { get; }

    int Execute();
}