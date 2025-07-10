using Avalonia.Media;
using Material.Icons;

namespace Aurelio.Public.Module.Ui;

public class Icon
{
    public static StreamGeometry FromMaterial(MaterialIconKind kind)
    {
        return StreamGeometry.Parse(MaterialIconDataProvider.GetData(kind));
    }
}