using Avalonia.Media;
using Material.Icons;

namespace Aurelio.Public.Module.Ui;

public class Icons
{
    public static StreamGeometry FromMaterial(MaterialIconKind kink)
    {
        return StreamGeometry.Parse(MaterialIconDataProvider.GetData(kink));
    }
}