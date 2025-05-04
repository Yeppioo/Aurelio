using Aurelio.Plugin.Base;
using Avalonia.Media.Imaging;

namespace Aurelio.Plugin.Simple;

public class Third: IPlugin
{
    public string Name => "Plugin.Simple.Third";
    public string Author => "Yep";
    public string Description => "Yes, a dll can contain multiple plugins.";
    public string Version => "ヾ(•ω•`)o";
    public Bitmap Icon => Aurelio.Public.Module.Value.Converter.Base64ToBitmap(Simple.Icon.Third);

    public int Execute()
    {
        Console.WriteLine("Secondary plugin loaded successfully !");
        return 0;
    }
}