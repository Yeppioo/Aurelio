using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using System.Linq;

namespace Aurelio.Public.Media.Converter;

public sealed class FamilyTypefacesConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is FontFamily fontFamily)
            return string.Join('，', fontFamily.FamilyTypefaces.Select(x => x.Weight).Distinct());

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
