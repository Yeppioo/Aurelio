using Avalonia;
using Avalonia.Media;

namespace Aurelio.Plugin.Api.Helper;

public class Converter
{
    public static IImage PathToImage(StreamGeometry path)
    {
        var drawingImage = new DrawingImage();
        var geometryDrawing = new GeometryDrawing();
        geometryDrawing.Geometry = path;
        drawingImage.Drawing = geometryDrawing;
        Application.Current.TryGetResource("TextColor", Application.Current.ActualThemeVariant, out var color);
        geometryDrawing.Brush = (SolidColorBrush)color;
        Application.Current.ActualThemeVariantChanged += (_, _) =>
        {
            Application.Current.TryGetResource("TextColor", Application.Current.ActualThemeVariant, out var color);
            geometryDrawing.Brush = (SolidColorBrush)color;
        };
        return drawingImage;
    }
}