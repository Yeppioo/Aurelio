using System.Globalization;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class DoubleToIntByRoundConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return System.Convert.ToInt32(Math.Round(d));
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            return System.Convert.ToDouble(i);
        }

        return 0;
    }
}