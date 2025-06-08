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
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Aurelio.Public.Module;
using Avalonia;
using Avalonia.Threading;
using System.Windows.Input;
using Avalonia.Animation;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.Input;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontMappingTablePage : PageMixModelBase, IFunctionPage , IExpandablePage
{
    private SKTypeface skTypeface;
    private FontWeight _selectedFontWeight;
    private bool _fl = true;
    private FontStyle _selectedFontStyle;
    private Debouncer _borderAnimation;
    private string _showText = "你好 Hello 1234567890 ?!";
    private string _searchText = string.Empty;
    private string _currentChar = string.Empty;
    private List<CharacterBlock> _allBlocks;

    public TabEntry HostTab { get; set; }
    public FontFamily Current { get; set; }

    [RelayCommand]
    public void SelectAllFilters()
    {
        foreach (var filter in BlockFilters)
        {
            filter.IsChecked = true;
        }
        // ApplyBlockFilter已在属性更改事件中调用
    }

    [RelayCommand]
    public void DeselectAllFilters()
    {
        foreach (var filter in BlockFilters)
        {
            filter.IsChecked = false;
        }
        // ApplyBlockFilter已在属性更改事件中调用
    }

    public FontWeight SelectedFontWeight
    {
        get => _selectedFontWeight;
        set
        {
            SetField(ref _selectedFontWeight, value);
            LoadCharacters();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetField(ref _searchText, value);
            LoadCharacters();
        }
    }

    public string ShowText
    {
        get => _showText;
        set
        {
            SetField(ref _showText, value);
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
        BlockFilters.CollectionChanged += BlockFilters_CollectionChanged;
        _borderAnimation = new Debouncer(BorderAnimationAction, 250);
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

    public ObservableCollection<BlockFilterItem> BlockFilters { get; set; } = new();

    private void BlockFilters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (BlockFilterItem item in e.NewItems)
            {
                item.PropertyChanged += BlockFilterItem_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (BlockFilterItem item in e.OldItems)
            {
                item.PropertyChanged -= BlockFilterItem_PropertyChanged;
            }
        }
    }

    private void BlockFilterItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BlockFilterItem.IsChecked))
            ApplyBlockFilter();
    }

    public void LoadCharacters()
    {
        var style = new SKFontStyle((SKFontStyleWeight)SelectedFontWeight,
            SKFontStyleWidth.Normal, (SKFontStyleSlant)SelectedFontStyle);
        skTypeface = SKTypeface.FromFamilyName(Current.Name, style);
        var supportCharacters = GetSupportCharacter();
        BlockFilters.Clear();
        if (BlockFilters.Count == 0)
        {
            // 添加特殊过滤条目：完全匹配和名称匹配
            var exactMatchFilter = new BlockFilterItem { Name = MainLang.PerfectlyMatchedCharacters, IsChecked = true };
            exactMatchFilter.PropertyChanged += BlockFilterItem_PropertyChanged;
            BlockFilters.Add(exactMatchFilter);

            var nameMatchFilter = new BlockFilterItem { Name = MainLang.NameMatchedCharacters, IsChecked = true };
            nameMatchFilter.PropertyChanged += BlockFilterItem_PropertyChanged;
            BlockFilters.Add(nameMatchFilter);

            // 添加原有区块过滤
            foreach (var block in supportCharacters.CharacterBlocks.Select(b => b.Name).Distinct())
            {
                var filter = new BlockFilterItem { Name = block, IsChecked = true };
                filter.PropertyChanged += BlockFilterItem_PropertyChanged;
                BlockFilters.Add(filter);
            }
        }

        _allBlocks = supportCharacters.CharacterBlocks;
        ApplyBlockFilter();
        SelectedCharacter(_allBlocks.FirstOrDefault()?.Characters?.FirstOrDefault(x => x.Char == "!"));
    }

    public void ApplyBlockFilter()
    {
        if (CharacterBlocks == null) return;
        CharacterBlocks.Clear();

        // 获取选中的区块名称
        var selectedBlocks = BlockFilters.Where(b => b.IsChecked).Select(b => b.Name).ToHashSet();

        // 检查特殊过滤条件是否被选中
        bool includeExactMatch = selectedBlocks.Contains(MainLang.PerfectlyMatchedCharacters);
        bool includeNameMatch = selectedBlocks.Contains(MainLang.NameMatchedCharacters);

        // 过滤掉特殊过滤条件，只保留实际的Unicode区块名称
        var filteredBlockNames = selectedBlocks
            .Where(name => name != MainLang.PerfectlyMatchedCharacters && name != MainLang.NameMatchedCharacters)
            .ToHashSet();

        // 应用区块过滤
        var filteredBlocks = _allBlocks.Where(b => filteredBlockNames.Contains(b.Name)).ToList();

        // 应用搜索过滤
        var searched = ApplySearchFilter(filteredBlocks, includeExactMatch, includeNameMatch);
        CharacterBlocks.AddRange(searched);
        ScrollViewer.ScrollToHome();
    }

    private List<CharacterBlock> ApplySearchFilter(List<CharacterBlock> blocks, bool includeExactMatch = true,
        bool includeNameMatch = true)
    {
        var keyword = SearchText.Replace(" ", "").ToLowerInvariant();
        if (string.IsNullOrEmpty(keyword))
            return blocks;

        var exactMatch = new List<CharacterEntry>();
        var nameMatch = new List<CharacterEntry>();
        var fuzzyDict = new Dictionary<string, List<CharacterEntry>>();

        var exactSet = new HashSet<CharacterEntry>();
        var nameSet = new HashSet<CharacterEntry>();

        // 处理U+XXXX格式的搜索
        bool isUnicodeCodePointSearch = false;
        int codePointValue = 0;

        // 匹配U+XXXX格式（不区分大小写）
        if (keyword.StartsWith("u+") && keyword.Length > 2)
        {
            string hexPart = keyword.Substring(2);
            // 尝试解析16进制值
            if (int.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out codePointValue))
            {
                isUnicodeCodePointSearch = true;
            }
        }

        foreach (var block in blocks)
        {
            foreach (var entry in block.Characters)
            {
                // 如果是Unicode代码点搜索，则进行精确匹配
                if (isUnicodeCodePointSearch)
                {
                    if (entry.Code == codePointValue)
                    {
                        exactMatch.Add(entry);
                        exactSet.Add(entry);
                        continue; // 找到精确匹配后，跳过其他匹配
                    }
                }
                else // 非Unicode代码点搜索，使用常规匹配逻辑
                {
                    var charKey = entry.Char.Replace(" ", "").ToLowerInvariant();
                    var nameKey = (entry.Name).Replace(" ", "").ToLowerInvariant();

                    if (charKey == keyword)
                    {
                        exactMatch.Add(entry);
                        exactSet.Add(entry);
                    }
                    else if (nameKey.Contains(keyword))
                    {
                        nameMatch.Add(entry);
                        nameSet.Add(entry);
                    }
                    else if (
                        charKey.Contains(keyword) ||
                        nameKey.Contains(keyword) ||
                        entry.Code.ToString().Contains(keyword) ||
                        entry.Code.ToString("X").ToLowerInvariant().Contains(keyword) ||
                        ("u+" + entry.Code.ToString("X")).ToLowerInvariant().Contains(keyword)
                    )
                    {
                        if (!fuzzyDict.TryGetValue(block.Name, out var list))
                        {
                            list = new List<CharacterEntry>();
                            fuzzyDict[block.Name] = list;
                        }

                        // 避免和 exact/name 重复
                        if (!exactSet.Contains(entry) && !nameSet.Contains(entry))
                            list.Add(entry);
                    }
                }
            }
        }

        var result = new List<CharacterBlock>();

        // 根据过滤条件添加完全匹配
        if (exactMatch.Count > 0 && includeExactMatch)
        {
            result.Add(new CharacterBlock
            {
                Name = MainLang.PerfectlyMatchedCharacters,
                Characters = exactMatch
            });
        }

        // 非Unicode代码点搜索时，才考虑名称匹配
        if (!isUnicodeCodePointSearch && nameMatch.Count > 0 && includeNameMatch)
        {
            result.Add(new CharacterBlock
            {
                Name = MainLang.NameMatchedCharacters,
                Characters = nameMatch
            });
        }

        // 非Unicode代码点搜索时，才考虑模糊匹配
        if (!isUnicodeCodePointSearch)
        {
            // 添加原始区块匹配结果
            foreach (var kv in fuzzyDict)
            {
                if (kv.Value.Count > 0)
                {
                    result.Add(new CharacterBlock
                    {
                        Name = kv.Key,
                        Characters = kv.Value
                    });
                }
            }
        }

        // 如果没有任何匹配结果，返回原始区块
        if (result.Count == 0)
        {
            return blocks;
        }

        return result;
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

        _allBlocks = supportCharacter.CharacterBlocks;
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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var character = (CharacterEntry)((TextBlock)sender).Tag!;
        SelectedCharacter(character);
    }

    private void SelectedCharacter(CharacterEntry? character)
    {
        if (character == null) return;
        var charInfo = UnicodeInfo.GetCharInfo(character.Code);
        SCName.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(character.Name.ToLower());
        SCInfo.Text = $"{character.Hex}, {charInfo.Block}";

        SCXaml.Text = $"&#x{character.Code:X4};";
        SCXaml.CaretIndex = 0;

        SCFontIcon.Text = $"<FontIcon FontFamily=\"{Current.Name}\" Glyph=\"&#x{character.Code:X4};\" />";
        SCFontIcon.CaretIndex = 0;

        // Label.Content = character.Char;
        _currentChar = character.Char;
        Draw(character);
    }

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
    }

    public class BlockFilterItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private bool _isChecked = true;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    private void BorderAnimationAction()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (Border.IsVisible)
            {
                Border.Margin = new Thickness(0, Bounds.Height, 0, 0);
                await Task.Delay(250);
                Border.IsVisible = false;
            }
            else
            {
                Border.IsVisible = true;
                Border.Margin = new Thickness(0);
            }
        });
    }

    private void ToggleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _borderAnimation.Trigger();
    }

    private void Copy_OnClick(object? sender, RoutedEventArgs e)
    {
        var tag = (string)((Button)sender).Tag!;
        var board = App.TopLevel.Clipboard;
        if (tag == "SCPathIcon")
        {
            board.SetTextAsync(SCPathIcon.Text);
        }
        else if (tag == "SCXaml")
        {
            board.SetTextAsync(SCXaml.Text);
        }
        else if (tag == "SCFontIcon")
        {
            board.SetTextAsync(SCFontIcon.Text);
        }
    }

    [RelayCommand]
    public async void ExportSvg()
    {
        try
        {
            var path = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = MainLang.Export,
                SuggestedFileName = $"{Current.Name} {SelectedFontWeight} {SelectedFontStyle} - {SCName.Text}.svg",
                DefaultExtension = ".svg",
                FileTypeChoices =
                [
                    new FilePickerFileType("Svg File") { Patterns = ["*.svg"] }
                ]
            });
            if (path == null) return;
            await File.WriteAllTextAsync(path.Path.LocalPath, Svg.Source);
            Shower.Notice(MainLang.ExportFinish, NotificationType.Success, () => { App.TopLevel.Launcher.LaunchFileAsync(path); });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Shower.Notice(MainLang.ExportFail, NotificationType.Error);
        }
    }

    [RelayCommand]
    public async void ExportPng()
    {
        try
        {
            var path = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = MainLang.Export,
                SuggestedFileName = $"{Current.Name} {SelectedFontWeight} {SelectedFontStyle} - {SCName.Text}.png",
                DefaultExtension = ".png",
                FileTypeChoices =
                [
                    new FilePickerFileType("PNG Image") { Patterns = ["*.png"] }
                ]
            });
            if (path == null) return;
            
            // 创建SkiaSharp绘图表面
            using var paint = new SKPaint();
            paint.Typeface = skTypeface;
            paint.TextSize = 1000;
            paint.IsAntialias = true;
            paint.Color = SKColors.Black;

            // 获取字形路径
            using var path1 = paint.GetTextPath(_currentChar, 0, 1000);
            var bounds = path1.Bounds;

            // 计算居中位置
            float svgCenterX = 500;
            float svgCenterY = 750;
            var centerX = bounds.Left + bounds.Width / 2;
            var centerY = bounds.Top + bounds.Height / 2;
            var translateX = svgCenterX - centerX;
            var translateY = svgCenterY - centerY;

            // 创建PNG图像
            using var bitmap = new SKBitmap(1000, 1500);
            using var canvas = new SKCanvas(bitmap);

            // 绘制白色背景
            canvas.Clear(SKColors.Empty);

            // 应用变换并绘制字形
            canvas.Translate(translateX, translateY);
            canvas.DrawPath(path1, paint);

            // 保存为PNG
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(path.Path.LocalPath);
            data.SaveTo(stream);
            
            Shower.Notice(MainLang.ExportFinish, NotificationType.Success, () => { App.TopLevel.Launcher.LaunchFileAsync(path); });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Shower.Notice(MainLang.ExportFail, NotificationType.Error);
        }
    }

    [RelayCommand]
    public async void ExportXamlImageSource()
    {
        try
        {
            var path = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = MainLang.Export,
                SuggestedFileName = $"{Current.Name} {SelectedFontWeight} {SelectedFontStyle} - {SCName.Text}.xaml",
                DefaultExtension = ".xaml",
                FileTypeChoices =
                [
                    new FilePickerFileType("XAML File") { Patterns = ["*.xaml"] }
                ]
            });
            if (path == null) return;
            
            // 创建SkiaSharp绘图表面
            using var paint = new SKPaint();
            paint.Typeface = skTypeface;
            paint.TextSize = 1000;
            paint.IsAntialias = true;

            // 获取字形路径
            using var textPath = paint.GetTextPath(_currentChar, 0, 1000);
            var pathData = textPath.ToSvgPathData();

            // 格式化为完整的XAML Image代码
            var xamlSource = "<Image xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n" +
                            "       xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
                            "       Width=\"100\" Height=\"100\">\n" +
                            "  <Image.Source>\n" +
                            "    <DrawingImage>\n" +
                            "      <DrawingImage.Drawing>\n" +
                            "        <DrawingGroup>\n" +
                            $"          <GeometryDrawing Brush=\"Black\" Geometry=\"{pathData}\" />\n" +
                            "        </DrawingGroup>\n" +
                            "      </DrawingImage.Drawing>\n" +
                            "    </DrawingImage>\n" +
                            "  </Image.Source>\n" +
                            "</Image>";

            await File.WriteAllTextAsync(path.Path.LocalPath, xamlSource);
            Shower.Notice(MainLang.ExportFinish, NotificationType.Success, () => { App.TopLevel.Launcher.LaunchFileAsync(path); });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Shower.Notice(MainLang.ExportFail, NotificationType.Error);
        }
    }

    public void OpenInNewWindow()
    {
        var fontMappingTablePage = new FontMappingTablePage(Current);
        fontMappingTablePage.Card.Margin = new Thickness(20,0,20,20);
        var win = new AurelioWindow(fontMappingTablePage, PageInfo.Title);
        win.Show();
    }
}