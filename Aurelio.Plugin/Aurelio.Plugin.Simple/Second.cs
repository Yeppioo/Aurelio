using Aurelio.Plugin.Base;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Aurelio.Plugin.Simple;

public class Secondary : IPlugin
{
    public string Name => "Plugin.Simple.Second";
    public string Author => "Yep";
    public string Description => "Yes, a dll can contain multiple plugins.";
    public string Version => "1.0.23";
    public IImage Icon => Aurelio.Public.Module.Value.Converter.Base64ToBitmap(Simple.Icon.Secondary);

    public int Execute()
    {
        Console.WriteLine("Secondary plugin loaded successfully !");
        return 0;
    }
}