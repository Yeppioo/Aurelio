using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class ContainerWidthDividerConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return 280.0; // 默认宽度，确保返回 double

        double containerWidth = 0;
        int count = 3; // 默认每行3个

        // 尝试获取容器宽度
        if (values[0] is double width)
            containerWidth = width;
        else if (values[0] is int intWidth)
            containerWidth = intWidth;
        else if (values[0] is string widthStr && double.TryParse(widthStr, out double parsedWidth))
            containerWidth = parsedWidth;
            
        // 根据容器宽度计算每行显示的卡片数量
        if (containerWidth <= 0)
            return 280.0; // 容器宽度无效，返回默认宽度
            
        // 根据规则计算每行卡片数
        if (containerWidth <= 450)
            count = 1;
        else if (containerWidth <= 650)
            count = 2;
        else if (containerWidth <= 850)
            count = 3;
        else
        {
            // 使用公式 y = 250X + 150 计算，反向求解 X = (y - 150) / 250
            count = Math.Max(3, (int)Math.Floor((containerWidth - 150) / 250));
        }
            
        // 如果有指定数量的第二个参数，则使用指定的数量
        if (values[1] is double dblCount && dblCount > 0)
            count = (int)dblCount;
        else if (values[1] is int intCount && intCount > 0)
            count = intCount;
        else if (values[1] is string countStr)
        {
            if (int.TryParse(countStr, out int parsedCount) && parsedCount > 0)
                count = parsedCount;
            else if (double.TryParse(countStr, out double parsedDblCount) && parsedDblCount > 0)
                count = (int)parsedDblCount;
        }
            
        double itemWidth = Math.Floor((containerWidth / count) - 10); // 减去边距，取整
        return Math.Max(itemWidth, 200.0); // 设置最小宽度为200，确保返回 double
    }
}