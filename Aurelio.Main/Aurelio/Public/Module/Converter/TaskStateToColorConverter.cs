using System.Globalization;
using Aurelio.Public.Classes.Enum;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Aurelio.Public.Module.Converter;

public class TaskStateToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskState state)
            return state switch
            {
                TaskState.Paused => new SolidColorBrush(Color.Parse("#fce100")),
                TaskState.Canceled => new SolidColorBrush(Color.Parse("#fce100")),
                TaskState.Canceling => new SolidColorBrush(Color.Parse("#fce100")),
                TaskState.Error => new SolidColorBrush(Color.Parse("#ff99a4")),
                TaskState.Finished => new SolidColorBrush(Color.Parse("#00fc40")),
                _ => new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]!)
            };
        return new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]!);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}