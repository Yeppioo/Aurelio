using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Aurelio.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DynamicData;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Unicode;
using Aurelio.Public.Classes.Entries.Functions.FontMapping;
using Aurelio.Public.Classes.Entries.Page;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontMappingTablePage : PageMixModelBase, IFunctionPage
{
    private SKTypeface skTypeface;
    private FontWeight _selectedFontWeight;
    private bool _fl = true;
    private FontStyle _selectedFontStyle;

    public TabEntry HostTab { get; set; }
    public FontFamily Current { get; set; }

    public FontWeight SelectedFontWeight
    {
        get => _selectedFontWeight;
        set
        {
            SetField(ref _selectedFontWeight, value);
            LoadCharacters();
        }
    }

    public FontStyle SelectedFontStyle
    {
        get => _selectedFontStyle;
        set
        {
            SetField(ref _selectedFontStyle, value);
            LoadCharacters();
        }
    }

    public static IFunctionPage? RecentOpenHandle(RecentPageEntry entry)
    {
        if (Convert.ToInt32(entry.Data) == 0)
        {
            var font = FontManager.Current.SystemFonts.FirstOrDefault(x => x.FamilyNames.Contains(entry.FilePath));
            return font == null ? null : new FontMappingTablePage(font);
        }

        return null;
    }

    public FontMappingTablePage(FontFamily fontFamily)
    {
        Current = fontFamily;
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!_fl) return;
        _fl = false;
        SelectedFontWeight = Current.FamilyTypefaces.Select(x => x.Weight).FirstOrDefault();
        LoadCharacters();
        FunctionConfig.AddRecentOpen(new RecentPageEntry()
        {
            Title = PageInfo.Title,
            Summary = $"{MainLang.SystemFont}: {Current}",
            FilePath = Current.ToString(),
            Data = FontFamilyType.System,
            FunctionType = FunctionType.CharacterMapping
        });
    }

    public ObservableCollection<CharacterBlock>? CharacterBlocks { get; set; } = [];

    public void LoadCharacters()
    {
        var style = new SKFontStyle((SKFontStyleWeight)SelectedFontWeight,
            SKFontStyleWidth.Normal, (SKFontStyleSlant)SelectedFontStyle);
        skTypeface = SKTypeface.FromFamilyName(Current.Name, style);
        CharacterBlocks.Clear();
        var supportCharacters = GetSupportCharacter();
        CharacterBlocks.AddRange(supportCharacters.CharacterBlocks);
    }

    public SupportCharacter GetSupportCharacter()
    {
        var supportCharacter = new SupportCharacter
        {
            Name = Current.Name,
            CharacterBlocks = []
        };

        var typeface = skTypeface;
        var blockDict = new Dictionary<string, List<CharacterEntry>>();
        CharacterCountTextBlock.Text = MainLang.CharacterCount.Replace("{num}", typeface.GlyphCount.ToString());

        const int batchSize = 256;
        var codepoints = new List<int>(0x10000);
        for (int i = 0; i <= 0xFFFF; i++)
        {
            if (i is >= 0xD800 and <= 0xDFFF) continue; // 跳过 surrogate code unit
            codepoints.Add(i);
        }

        for (int batchStart = 0; batchStart < codepoints.Count; batchStart += batchSize)
        {
            var batch = codepoints.Skip(batchStart)
                .Take(batchSize).ToArray();
            var glyphs = typeface.GetGlyphs(batch);
            for (int j = 0; j < batch.Length; j++)
            {
                if (glyphs[j] == 0) continue; // 没有字形
                var i = batch[j];
                var charInfo = UnicodeInfo.GetCharInfo(i);
                var s = char.ConvertFromUtf32(i);
                var block = charInfo.Block;

                if (!blockDict.ContainsKey(block))
                    blockDict[block] = [];

                blockDict[block].Add(new CharacterEntry
                {
                    Char = s,
                    Code = i,
                    Name = charInfo.Name ?? charInfo.OldName
                });
            }
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

    public void OnClose()
    {
        DataContext = null;
        skTypeface?.Dispose();
        skTypeface = null;
        if (CharacterBlocks != null)
        {
            CharacterBlocks.Clear();
            CharacterBlocks = null;
        }

        Current = null;
        Loaded -= OnLoaded;
        GC.SuppressFinalize(this);
    }

    public PageInfoEntry PageInfo => new()
    {
        Title = $"{MainLang.CharacterMapping}: {Current.Name}",
        Icon = Icons.CharacterAppearance
    };

    [Obsolete("Obsolete")]
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var character = (CharacterEntry)((TextBlock)sender).Tag!;

        var charInfo = UnicodeInfo.GetCharInfo(character.Code);
        SCName.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(character.Name.ToLower());
        SCInfo.Text = $"{character.Hex}, {charInfo.Block}";
        SCXaml.Text = $"&#x{character.Code:X4};";
        SCFontIcon.Text = $"<FontIcon FontFamily=\"{Current.Name}\" Glyph=\"&#x{character.Code:X4};\" />";

        // Label.Content = character.Char;
        Draw(character);
    }

    [Obsolete("Obsolete")]
    private void Draw(CharacterEntry? character)
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

        Svg.Source = svg;

        // var c = new ContentDialog()
        // {
        //     CloseButtonText = "Ok",
        //     Content = new SelectableTextBlock() { TextWrapping = TextWrapping.Wrap, Text = svg }
        // };
        // c.ShowAsync();
    }
}