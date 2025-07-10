using System.Globalization;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class BoolToDoubleConverter : IValueConverter
{
    public static BoolToDoubleConverter Instance { get; } = new();

    public double TrueValue { get; set; } = 1.0;
    public double FalseValue { get; set; } = 0.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue) return boolValue ? TrueValue : FalseValue;
        return FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue) return Math.Abs(doubleValue - TrueValue) < 0.1;
        return false;
    }
}