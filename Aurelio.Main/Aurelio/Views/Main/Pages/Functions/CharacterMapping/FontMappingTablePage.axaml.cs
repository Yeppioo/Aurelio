using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Unicode;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
using Aurelio.Public.Classes.Types;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DynamicData;
using SkiaSharp;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontMappingTablePage : UserControl, IFunctionPage
{
    private SKTypeface skTypeface;
    private bool _fl = true;
    public RecordFontFamilyEntry Entry { get; set; }

    public FontMappingTablePage(RecordFontFamilyEntry entry)
    {
        Entry = entry;
        InitializeComponent();
        DataContext = this;
        SelectedTypeface = (RecordTypefaceEntry)Entry.Typefaces.FirstOrDefault();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!_fl) return;
        _fl = false;
        LoadCharacters(SelectedTypeface.Path);
    }

    public ObservableCollection<CharacterBlock>? CharacterBlocks { get; set; } = [];

    public void LoadCharacters(string path)
    {
        CharacterBlocks.Clear();
        var supportCharacters = GetSupportCharacter(path);
        CharacterBlocks.AddRange(supportCharacters.CharacterBlocks);
        DrawInfo(CharacterBlocks?.FirstOrDefault()?.Characters?.FirstOrDefault()!);
    }

    public SupportCharacter GetSupportCharacter(string path)
    {
        var supportCharacter = new SupportCharacter
        {
            Name = Entry.DisplayName,
            CharacterBlocks = []
        };
        var blockDict = new Dictionary<string, List<CharacterEntry>>();
        skTypeface = SKTypeface.FromFile(path);
        CharacterCountTextBlock.Text = MainLang.CharacterCount.Replace("{num}", skTypeface.GlyphCount.ToString());

        for (var i = 0; i <= 0xFFFF; i++)
        {
            if (i is >= 0xD800 and <= 0xDFFF) continue; // 跳过 surrogate code unit

            var charInfo = UnicodeInfo.GetCharInfo(i);

            var s = char.ConvertFromUtf32(i);
            if (!skTypeface.ContainsGlyph(i)) continue;
            var block = charInfo.Block;
            if (!blockDict.ContainsKey(block))
                blockDict[block] = [];
            blockDict[block].Add(new CharacterEntry
                { Char = s, Code = i, Name = (charInfo.Name ?? charInfo.OldName) });
        }

        foreach (var kv in blockDict)
        {
            supportCharacter.CharacterBlocks.Add(new CharacterBlock
            {
                Name = kv.Key,
                Characters = kv.Value
            });
        }

        return supportCharacter;
    }


    public (string title, StreamGeometry icon) GetPageInfo()
    {
        return ($"{MainLang.CharacterMapping}: {Entry.DisplayName}", Icons.CharacterAppearance);
    }

    public TabEntry HostTab { get; set; }
    public UserControl HostContent { get; set; }

    public RecordTypefaceEntry SelectedTypeface
    {
        get => _selectedTypeface;
        set
        {
            SetField(ref _selectedTypeface, value);
            LoadCharacters(SelectedTypeface.Path);
        }
    }

    private RecordTypefaceEntry _selectedTypeface;

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    [Obsolete("Obsolete")]
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var character = (CharacterEntry)((TextBlock)sender).Tag!;
        DrawInfo(character);
    }

    [Obsolete("Obsolete")]
    private void DrawInfo(CharacterEntry? character)
    {
        if (character == null) return;

        using var paint = new SKPaint();
        paint.Typeface = skTypeface;
        paint.TextSize = 1000;

        var glyphs = paint.GetGlyphs(character.Char);
        if (glyphs.Length <= 0) return;
        // using var stream = new MemoryStream();
        // using var writer = new StreamWriter("glyph.svg");
        using var path = paint.GetTextPath(character.Char, 0, 1000);
        var bounds = path.Bounds; // 获取 path 的边界

        var centerX = bounds.Left + bounds.Width / 2;
        var centerY = bounds.Top + bounds.Height / 2;

        float svgCenterX = 500;
        float svgCenterY = 750; // 如果 viewBox 是 1000x1500

        var translateX = svgCenterX - centerX;
        var translateY = svgCenterY - centerY;

        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='1000' height='1500' viewBox='0 0 1000 1500'>" +
                  $"<g transform='translate({translateX}, {translateY})'>" +
                  $"<path d='{path.ToSvgPathData()}' fill='black'/></g></svg>";

        var charInfo = UnicodeInfo.GetCharInfo(character.Code);
        SCName.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(character.Name.ToLower());
        SCInfo.Text = $"{character.Hex}, {charInfo.Block}";
        SCXaml.Text = $"&#x{character.Code:X4};";
        SCFontIcon.Text = $"<FontIcon FontFamily=\"{Entry.FontFamilyName}\" Glyph=\"&#x{character.Code:X4};\" />";

        Svg.Source = svg;
    }

    public void Dispose()
    {
        DataContext = null;
        skTypeface?.Dispose();
        skTypeface = null;
        if (CharacterBlocks != null)
        {
            CharacterBlocks.Clear();
            CharacterBlocks = null;
        }
        Entry = null;
        Loaded -= OnLoaded;
        GC.SuppressFinalize(this);
    }
}