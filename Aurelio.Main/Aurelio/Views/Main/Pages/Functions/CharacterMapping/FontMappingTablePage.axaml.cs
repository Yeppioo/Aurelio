using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
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

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontMappingTablePage : PageMixModelBase, IFunctionPage
{
    private SKTypeface skTypeface;
    private FontWeight _selectedFontWeight;

    public TabEntry HostTab { get; set; }
    public FontFamily Current { get; set; }
    public UserControl HostContent { get; set; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public FontWeight SelectedFontWeight {
        get => _selectedFontWeight;
        set {
            SetField(ref _selectedFontWeight, value);
            Label.FontWeight = value;
            LoadCharacters();
        }
    }

    public FontMappingTablePage(FontFamily fontFamily) {
        Current = fontFamily;
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
        FunctionConfig.AddRecentOpen(new RecentOpenEntry()
        {
            Title = PageInfo.Title,
            Summary = fontFamily.ToString(),
            FilePath = fontFamily.ToString(),
            FunctionType = FunctionType.CharacterMapping
        });
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) {
        SelectedFontWeight = Current.FamilyTypefaces.Select(x => x.Weight).FirstOrDefault();
        LoadCharacters();
    }

    public ObservableCollection<CharacterBlock>? CharacterBlocks { get; set; } = [];

    public void LoadCharacters() {
        CharacterBlocks.Clear();
        var supportCharacters = GetSupportCharacter();
        CharacterBlocks.AddRange(supportCharacters.CharacterBlocks);
    }

    public (string title, StreamGeometry icon) GetPageInfo() {
        return ($"{MainLang.CharacterMapping}: {Current.Name}", Icons.CharacterAppearance);
    }

    public SupportCharacter GetSupportCharacter() {
        var supportCharacter = new SupportCharacter {
            Name = Current.Name,
            CharacterBlocks = []
        };

        var skTypeface = SKTypeface.FromFamilyName(Current.Name);
        var blockDict = new Dictionary<string, List<CharacterEntry>>();
        CharacterCountTextBlock.Text = MainLang.CharacterCount.Replace("{num}", skTypeface.GlyphCount.ToString());

        const int batchSize = 256;
        var codepoints = new List<int>(0x10000);
        for (int i = 0; i <= 0xFFFF; i++) {
            if (i is >= 0xD800 and <= 0xDFFF) continue; // 跳过 surrogate code unit
            codepoints.Add(i);
        }

        for (int batchStart = 0; batchStart < codepoints.Count; batchStart += batchSize) {
            var batch = codepoints.Skip(batchStart)
                .Take(batchSize).ToArray();

            var glyphs = skTypeface.GetGlyphs(batch);
            for (int j = 0; j < batch.Length; j++) {
                if (glyphs[j] == 0) continue; // 没有字形
                var i = batch[j];

                var charInfo = UnicodeInfo.GetCharInfo(i);
                var s = char.ConvertFromUtf32(i);
                var block = charInfo.Block;

                if (!blockDict.ContainsKey(block))
                    blockDict[block] = [];

                blockDict[block].Add(new CharacterEntry {
                    Char = s,
                    Code = i,
                    Name = charInfo.Name ?? charInfo.OldName
                });
            }
        }

        foreach (var kv in blockDict) {
            supportCharacter.CharacterBlocks.Add(new CharacterBlock {
                Name = kv.Key,
                Characters = kv.Value
            });
        }

        return supportCharacter;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
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

    public RecordTypefaceEntry SelectedTypeface
    {
        get => _selectedTypeface;
        set
        {
            SetField(ref _selectedTypeface, value);
            LoadCharacters();
        }
    }

    private RecordTypefaceEntry _selectedTypeface;

    [Obsolete("Obsolete")]
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var character = (CharacterEntry)((TextBlock)sender).Tag!;

        var charInfo = UnicodeInfo.GetCharInfo(character.Code);
        SCName.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(character.Name.ToLower());
        SCInfo.Text = $"{character.Hex}, {charInfo.Block}";
        SCXaml.Text = $"&#x{character.Code:X4};";
        SCFontIcon.Text = $"<FontIcon FontFamily=\"{Current.Name}\" Glyph=\"&#x{character.Code:X4};\" />";

        Label.Content = character.Char;
    }

    public void Dispose() {
        DataContext = null;
        skTypeface?.Dispose();
        skTypeface = null;
        if (CharacterBlocks != null) {
            CharacterBlocks.Clear();
            CharacterBlocks = null;
        }
        Current = null;
        Loaded -= OnLoaded;
        GC.Collect();
    }
}