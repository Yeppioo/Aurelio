using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Aurelio.Public.Module.Converter;

public sealed class FamilyTypefacesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontFamily fontFamily)
            return string.Join('，', fontFamily.FamilyTypefaces.Select(x => x.Weight).Distinct());

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}

public sealed class FamilyTypefacesToWeightListConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontFamily fontFamily)
            return fontFamily.FamilyTypefaces.Select(x => x.Weight).Distinct();

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}

public sealed class FamilyTypefacesToStyleListConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontFamily fontFamily)
            return fontFamily.FamilyTypefaces.Select(x => x.Style).Distinct();

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}