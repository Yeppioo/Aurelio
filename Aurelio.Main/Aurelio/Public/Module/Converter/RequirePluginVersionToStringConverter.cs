using System.Globalization;
using System.Linq;
using Aurelio.Plugin.Base;
using Avalonia.Data.Converters;

namespace Aurelio.Public.Module.Converter;

public class RequirePluginVersionToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not RequirePluginEntry entry) return null;

        // 如果没有版本要求，返回 "任意版本"
        if (entry.VersionRange == null || entry.VersionRange.Length == 0)
            return "任意版本";

        var version = entry.VersionRange[0];

        // 如果有版本范围（两个版本），处理范围显示
        if (entry.VersionRange.Length <= 1)
            return entry.RequireMethod switch
            {
                RequireMethod.Equal => $"= v{version}",
                RequireMethod.GreaterThan => $"> v{version}",
                RequireMethod.LessThan => $"< v{version}",
                RequireMethod.GreaterThanOrEqual => $"≥ v{version}",
                RequireMethod.LessThanOrEqual => $"≤ v{version}",
                _ => $"v{version}"
            };
        var minVersion = entry.VersionRange[0];
        var maxVersion = entry.VersionRange[1];
        return $"v{minVersion} - v{maxVersion}";

        // 单个版本的比较符号
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

    /// <summary>
    /// 静态方法，直接转换 RequirePluginEntry 为可视化字符串
    /// </summary>
    /// <param name="entry">插件依赖条目</param>
    /// <returns>可视化的版本要求字符串</returns>
    public static string ConvertToString(RequirePluginEntry entry)
    {
        if (entry == null) return "无要求";

        // 如果没有版本要求，返回 "任意版本"
        if (entry.VersionRange == null || entry.VersionRange.Length == 0)
            return "任意版本";

        var version = entry.VersionRange[0];

        // 如果有版本范围（两个版本），处理范围显示
        if (entry.VersionRange.Length > 1)
        {
            var minVersion = entry.VersionRange[0];
            var maxVersion = entry.VersionRange[1];
            return $"v{minVersion} - v{maxVersion}";
        }

        // 单个版本的比较符号
        return entry.RequireMethod switch
        {
            RequireMethod.Equal => $"= v{version}",
            RequireMethod.GreaterThan => $"> v{version}",
            RequireMethod.LessThan => $"< v{version}",
            RequireMethod.GreaterThanOrEqual => $"≥ v{version}",
            RequireMethod.LessThanOrEqual => $"≤ v{version}",
            _ => $"v{version}"
        };
    }

    /// <summary>
    /// 转换插件依赖数组为可视化字符串列表
    /// </summary>
    /// <param name="requirements">插件依赖数组</param>
    /// <returns>可视化的版本要求字符串列表</returns>
    public static string[] ConvertArrayToStrings(RequirePluginEntry[] requirements)
    {
        if (requirements == null || requirements.Length == 0)
            return new[] { "无依赖" };

        return requirements.Select(ConvertToString).ToArray();
    }

    /// <summary>
    /// 转换插件依赖数组为单个可视化字符串（用逗号分隔）
    /// </summary>
    /// <param name="requirements">插件依赖数组</param>
    /// <returns>用逗号分隔的版本要求字符串</returns>
    public static string ConvertArrayToSingleString(RequirePluginEntry[] requirements)
    {
        if (requirements == null || requirements.Length == 0)
            return "无依赖";

        return string.Join(", ", requirements.Select(req => $"{req.Id} ({ConvertToString(req)})"));
    }
}