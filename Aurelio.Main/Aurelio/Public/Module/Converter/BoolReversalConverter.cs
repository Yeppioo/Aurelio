using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class BoolReversalConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.FirstOrDefault() is bool b) return !b;

        return false;
    }
}