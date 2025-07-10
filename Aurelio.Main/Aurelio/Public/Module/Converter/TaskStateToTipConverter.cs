using System.Globalization;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class TaskStateToTipConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskState state)
        {
            return state switch
            {
                TaskState.Waiting => MainLang.Waiting,
                TaskState.Running => MainLang.Running,
                TaskState.Paused => MainLang.Paused,
                TaskState.Error => MainLang.Error,
                TaskState.Canceled => MainLang.Canceled,
                TaskState.Canceling => MainLang.Canceling,
                TaskState.Finished => MainLang.Finished,
                _ => state.ToString()
            };
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}