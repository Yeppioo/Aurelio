using Aurelio.Public.Module.Value;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using Avalonia.Visuals;

namespace Aurelio.Public.Module.Ui;

public class Icon
{
    public static StreamGeometry FromMaterial(MaterialIconKind kind)
    {
        return StreamGeometry.Parse(MaterialIconDataProvider.GetData(kind));
    }
}