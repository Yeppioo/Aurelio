using Aurelio.Plugin.Base;
using Avalonia.Media.Imaging;

namespace Aurelio.Plugin.Simple;

public class Secondary : IPlugin
{
    public string Name => "Plugin.Simple.Secondary";
    public string Author => "Yep";
    public string Description => "Yes, a dll can contain multiple plugins.";
    public string Version => "1.0.23";
    public Bitmap Icon => Aurelio.Public.Module.Value.Converter.Base64ToBitmap(Simple.Icon.Secondary);

    public int Execute()
    {
        Console.WriteLine("Secondary plugin loaded successfully !");
        return 0;
    }
}