using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Entries.Functions;

public record RecordFontFamilyEntry
{
    public string FontFamilyName { get; set; } = "Unknown";
    public string DisplayName { get; set; } = "Unknown";
    public string Styles { get; set; } = string.Empty;
    public FontFamily? FontFamily { get; set; }
    public List<RecordTypefaceEntry> Typefaces { get; set; } = [];
    public List<string> FilePaths { get; set; } = new();
}

public class RecordTypefaceEntry
{
    public string Name { get; set; } = string.Empty;
    public Typeface? Typeface { get; init; }
    public string Path { get; set; } = null;
    [Obsolete("Obsolete")] 
    public int Character => (int)Typeface?.GlyphTypeface.GlyphCount!;
}