using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SkiaSharp;

namespace Aurelio.Public.Module.Converter;

public class SkFontStyleToFontStyleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SKFontStyleSlant style || style == SKFontStyleSlant.Upright) return FontStyle.Normal;
        if (style == SKFontStyleSlant.Italic)
            return FontStyle.Italic;
        return style == SKFontStyleSlant.Oblique ? FontStyle.Oblique : FontStyle.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}