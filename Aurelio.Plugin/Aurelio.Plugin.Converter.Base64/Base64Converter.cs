using Aurelio.Plugin.Base;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Material.Icons;

namespace Aurelio.Plugin.Base64Converter;

public class Base64Converter : IPlugin
{
    public string Name => "Base64 and String Converter";
    public string Author => "Yep";
    public string Description => "Convert strings and base 64 to each other.";
    public string Version => "1.0.0";

    public IImage Icon =>
        Aurelio.Plugin.Api.Helper.Converter.PathToImage(
            Aurelio.Public.Content.Icons.FromMaterial(MaterialIconKind.AbTesting));

    public int Execute()
    {
        Aurelio.Plugin.Api.Instance.NavPages.Add(new Aurelio.Public.Classes.Entity.NavPage(
            "Yep.Aurelio.Plugin.Base64StringConverter", "Base64 Converter", new MainPage(),
            Aurelio.Public.Content.Icons.FromMaterial(MaterialIconKind.AbTesting),
            "Convert strings and base 64 to each other."));
        return 0;
    }
}