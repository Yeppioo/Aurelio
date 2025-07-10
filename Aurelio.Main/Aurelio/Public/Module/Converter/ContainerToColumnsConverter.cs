using System.Globalization;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

/// <summary>
///     根据容器宽度自动计算表格列数的转换器
/// </summary>
public class ContainerToColumnsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double containerWidth || containerWidth <= 0)
            return 3; // 默认3列

        // 根据容器宽度计算列数
        int columns;
        if (containerWidth <= 450)
            columns = 1;
        else if (containerWidth <= 650)
            columns = 2;
        else if (containerWidth <= 850)
            columns = 3;
        else
            // 使用公式 y = 250X + 150 计算列数，反向求解 X = (y - 150) / 250
            columns = Math.Max(3, (int)Math.Floor((containerWidth - 150) / 250));

        return columns;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}