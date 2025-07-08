using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Enum.Minecraft;
using Aurelio.Public.Langs;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Public.Module.Value;

public class Calculator
{
    public static Color ColorVariant(Color color, float percent)
    {
        // 确保百分比在-1到1之间  
        percent = Math.Max(-1f, Math.Min(1f, percent));

        // 计算调整后的RGB值  
        var adjust = 1f + percent; // 亮化是1+percent，暗化是1+(negative percent)，即小于1  
        var r = (int)Math.Round(color.R * adjust);
        var g = (int)Math.Round(color.G * adjust);
        var b = (int)Math.Round(color.B * adjust);

        // 确保RGB值在有效范围内  
        r = Math.Max(0, Math.Min(255, r));
        g = Math.Max(0, Math.Min(255, g));
        b = Math.Max(0, Math.Min(255, b));

        // 创建一个新的颜色（保持Alpha通道不变）  
        return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
    }

    public static Guid NameToMcOfflineUUID(string name)
    {
        var inputBytes = Encoding.UTF8.GetBytes("OfflinePlayer:" + name);
        var hash = MD5.HashData(inputBytes);

        hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash);
    }

    public static string FormatUsedTime(DateTime input, string longFormat = "yyyy-MM-dd")
    {
        if (input == DateTime.MinValue) return MainLang.NerverUsed;

        var timeDifference = DateTime.Now - input;

        if (!(timeDifference.TotalDays < 30)) return input.ToString(longFormat);
        var days = (int)Math.Floor(timeDifference.TotalDays);
        return MainLang.DaysAgo.Replace("{day}", days.ToString());
    }
}