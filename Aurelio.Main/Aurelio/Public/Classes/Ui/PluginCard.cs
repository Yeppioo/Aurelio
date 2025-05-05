using Avalonia;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Ui;

public class PluginCard
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public IImage Icon { get; set; }
    public Size Size { get; set; } = new(64, 64);
}