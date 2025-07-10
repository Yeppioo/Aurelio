using System.Globalization;
using Aurelio.Public.Langs;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class MinecraftInstanceSettingMemoryLimitConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double memoryLimit) return null;
        return memoryLimit < 0 ? MainLang.UseGlobalSetting : $"{memoryLimit} Mib";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}