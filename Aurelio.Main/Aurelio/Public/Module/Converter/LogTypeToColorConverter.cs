using System;
using System.Globalization;
using Aurelio.Public.Classes.Entries;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Aurelio.Public.Module.Converter;

public class LogTypeToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogType logType) return new SolidColorBrush(Colors.White);
        
        return logType switch
        {
            LogType.Error => new SolidColorBrush(Color.Parse("#FF0000")),
            LogType.Info => new SolidColorBrush(Colors.LightGreen),
            LogType.Debug => new SolidColorBrush(Colors.LightGray),
            LogType.Fatal => new SolidColorBrush(Color.Parse("#FF1493")),
            LogType.Warning => new SolidColorBrush(Color.Parse("#FFD700")),
            LogType.Exception => new SolidColorBrush(Color.Parse("#FF4500")),
            LogType.StackTrace => new SolidColorBrush(Color.Parse("#FF8C00")),
            LogType.Unknown => new SolidColorBrush(Colors.LightBlue),
            _ => new SolidColorBrush(Colors.White)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
} 