using Aurelio.Plugin.Base;
using Avalonia.Media.Imaging;

namespace Aurelio.Plugin.Simple;

public class Main : IPlugin
{
    public string Name => "Plugin.Simple.Main";
    public string Author => "Yep";
    public string Description => "A example plugin for Aurelio. This is a piece of information without actual content.";
    public string Version => "1.0.0";
    public Bitmap Icon => Aurelio.Public.Module.Value.Converter.Base64ToBitmap(Simple.Icon.Main);

    public int Execute()
    {
        Console.WriteLine("Example plugin loaded successfully !");
        return 0;
    }
}