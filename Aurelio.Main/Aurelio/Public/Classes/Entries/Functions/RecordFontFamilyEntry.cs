using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using SkiaSharp;

namespace Aurelio.Public.Classes.Entries.Functions;

public record RecordFontFamilyEntry
{
    public string FontFamilyName { get; set; } = "Unknown";
    public string DisplayName { get; set; } = "Unknown";
    public string StringStyles => string.Join(",", Styles.Select(FontStyleToString).ToList().Distinct());
    public FontFamily? FontFamily { get; set; }
    public List<RecordTypefaceEntry> Typefaces { get; set; } = [];
    public List<SKFontStyle> Styles { get; } = [];
    
    public static string FontStyleToString(SKFontStyle style)
    {
        // Weight
        var weightStr = style.Weight switch
        {
            <= 250 => "Thin",
            <= 350 => "Light",
            <= 450 => "Regular",
            <= 550 => "Medium",
            <= 650 => "SemiBold",
            <= 750 => "Bold",
            <= 850 => "ExtraBold",
            _ => "Black"
        };

        // Slant
        var slantStr = style.Slant switch
        {
            SKFontStyleSlant.Italic => "Italic",
            SKFontStyleSlant.Oblique => "Oblique",
            _ => ""
        };

        // 只在weight=Regular且slant=Upright时显示Regular
        if (weightStr == "Regular" && string.IsNullOrEmpty(slantStr))
            return "Regular";
        if (weightStr == "Regular")
            return slantStr;
        if (string.IsNullOrEmpty(slantStr))
            return weightStr;
        return $"{weightStr} {slantStr}";
    }
}

public class RecordTypefaceEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; init; }
    public SKFontStyle Style { get; init; }
}