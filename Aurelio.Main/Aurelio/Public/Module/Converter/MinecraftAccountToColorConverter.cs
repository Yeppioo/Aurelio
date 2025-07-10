using System.Globalization;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Aurelio.Public.Module.Converter;

public class MinecraftAccountToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not RecordMinecraftAccount account) return SolidColorBrush.Parse("#FF3434");
        return account.AccountType switch
        {
            Setting.AccountType.Microsoft => SolidColorBrush.Parse("#00FF40"),
            Setting.AccountType.Offline => SolidColorBrush.Parse("#FFA500"),
            Setting.AccountType.ThirdParty => SolidColorBrush.Parse("#35FFF6"),
            _ => SolidColorBrush.Parse("#00FF40")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}