using System;
using Avalonia.Media;

namespace Aurelio.Public.Module.Value;

public class Calculator
{
    public static Color ColorVariant(Color color, float percent)
    {
        percent = Math.Max(-1f, Math.Min(1f, percent));

        var adjust = 1f + percent;
        var r = (int)Math.Round(color.R * adjust);
        var g = (int)Math.Round(color.G * adjust);
        var b = (int)Math.Round(color.B * adjust);

        r = Math.Max(0, Math.Min(255, r));
        g = Math.Max(0, Math.Min(255, g));
        b = Math.Max(0, Math.Min(255, b));

        return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
    }
}