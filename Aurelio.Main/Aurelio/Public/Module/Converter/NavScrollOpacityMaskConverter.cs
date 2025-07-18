using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Aurelio.Public.Module.Converter;

public class NavScrollOpacityMaskConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((bool)value!)
        {
            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Colors.Transparent, 0.0),
                    new GradientStop(Colors.White, 0.04),
                    new GradientStop(Colors.White, 0.96),
                    new GradientStop(Colors.Transparent, 1.0),
                ]
            };
        }
        else
        {
            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Colors.White, 0.0),
                    new GradientStop(Colors.White, 0.08),
                    new GradientStop(Colors.White, 0.92),
                    new GradientStop(Colors.Transparent, 1.0),
                ]
            };
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}