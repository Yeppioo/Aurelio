using System.Globalization;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class ReverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(bool)value!;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(bool)value!;
    }
}