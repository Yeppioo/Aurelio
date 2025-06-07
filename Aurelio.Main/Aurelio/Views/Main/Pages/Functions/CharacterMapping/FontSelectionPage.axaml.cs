using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
using Aurelio.Public.Const;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using DynamicData;
using SkiaSharp;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Threading;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.ViewModels;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontSelectionPage : PageMixModelBase, IFunctionPage
{
    private bool _fl = true;
    private CancellationTokenSource _fontLoadCts;

    public FontSelectionPage()
    {
        InitializeComponent();
        DataContext = this;
        ListBox.SelectionChanged += ListBox_SelectionChanged;
        Loaded += LoadedHandler;
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ListBox.SelectedItem is not RecordFontFamilyEntry item) return;
        var page = new FontMappingTablePage(item);
        var text = new TextBlock();
        text.Inlines.Add(new Run($"{MainLang.CharacterMapping}: ") { FontFamily = item.FontFamily });
        text.Inlines.Add(new Run(item.DisplayName) { FontFamily = item.FontFamily });
        HostTab?.ReplacePage(page);
    }

    private void LoadedHandler(object? sender, EventArgs e)
    {
        if (!_fl) return;
        _fl = false;
        GetFonts();
        FilterFont();
    }

    public TabEntry HostTab { get; set; }

    public void OnClose()
    {
        if (ListBox != null)
            ListBox.SelectionChanged -= ListBox_SelectionChanged;
        Loaded -= LoadedHandler;

        Card.Content = null;

        FoundFonts?.Clear();
        FilteredFonts?.Clear();
        FoundFonts = null;
        FilteredFonts = null;

        _fontLoadCts?.Dispose();
        _fontLoadCts = null;
        DataContext = null;
        HostContent = null;
        HostTab = null;
        
        // FontManager.Current.RemoveFontCollection(new Uri("fonts:SystemFonts", UriKind.Absolute));

        GC.SuppressFinalize(this);
        GC.Collect(2);
    }

    public PageInfoEntry PageInfo => new()
    {
        Title = $"{MainLang.CharacterMapping}: {MainLang.FontList}",
        Icon = Icons.CharacterAppearance
    };

    public UserControl HostContent { get; set; }

    private string _searchFunctionText = string.Empty;

    public ObservableCollection<RecordFontFamilyEntry> FoundFonts { get; set; } = [];
    public ObservableCollection<RecordFontFamilyEntry> FilteredFonts { get; set; } = [];

    public string SearchFunctionText
    {
        get => _searchFunctionText;
        set
        {
            SetField(ref _searchFunctionText, value);
            FilterFont();
        }
    }

    private void GetFonts()
    {
        _fontLoadCts?.Cancel();
        _fontLoadCts = new CancellationTokenSource();
        var token = _fontLoadCts.Token;

        FoundFonts.Clear();
        Task.Run(() =>
        {
            var fontDirs = new List<string>();
            switch (Data.DesktopType)
            {
                case DesktopType.Windows:
                    fontDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"));
                    fontDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        @"Microsoft\\Windows\\Fonts"));
                    break;
                case DesktopType.MacOs:
                    fontDirs.Add("/System/Library/Fonts");
                    fontDirs.Add("/Library/Fonts");
                    fontDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Library/Fonts"));
                    break;
                case DesktopType.Linux:
                case DesktopType.FreeBSD:
                    fontDirs.Add("/usr/share/fonts");
                    fontDirs.Add("/usr/local/share/fonts");
                    fontDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".fonts"));
                    break;
                case DesktopType.Unknown:
                default:
                    break;
            }

            string[] fontExtensions = ["*.ttf", "*.otf", "*.ttc", "*.woff", "*.woff2"];
            var fontFiles = new List<string>();
            foreach (var dir in fontDirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var ext in fontExtensions)
                {
                    try
                    {
                        fontFiles.AddRange(Directory.GetFiles(dir, ext, SearchOption.AllDirectories));
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            var fontFamilyDict = new Dictionary<string, RecordFontFamilyEntry>(StringComparer.OrdinalIgnoreCase);
            var lockObj = new object();

            foreach (var file in fontFiles.Distinct())
            {
                try
                {
                    using var skTypeface = SKTypeface.FromFile(file);
                    if (skTypeface == null) return;
                    var familyName = skTypeface.FamilyName;
                    if (string.IsNullOrWhiteSpace(familyName)) return;

                    var name = RecordFontFamilyEntry.FontStyleToString(skTypeface.FontStyle);

                    lock (lockObj)
                    {
                        if (!fontFamilyDict.TryGetValue(familyName, out var entry))
                        {
                            entry = new RecordFontFamilyEntry
                            {
                                FontFamily = new FontFamily(familyName),
                                // FontFamily = null,
                                DisplayName =
                                    FontFamilyChineseNameTable.FontNameMap.TryGetValue(familyName, out var cn) &&
                                    !string.IsNullOrWhiteSpace(cn)
                                        ? $"{familyName} ({cn})"
                                        : familyName,
                                FontFamilyName = familyName,
                                Typefaces = []
                            };
                            fontFamilyDict[familyName] = entry;
                        }

                        if (entry.Typefaces.Any(x => x.Name == name && x.Path == file)) return;
                        entry.Typefaces.Add(new RecordTypefaceEntry
                        {
                            Name = name,
                            Path = file,
                            Style = skTypeface.FontStyle
                        });
                        entry.Styles.Add(skTypeface.FontStyle);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var fontFamilies = fontFamilyDict.Values.ToList();
            Dispatcher.UIThread.Post(() =>
            {
                FoundFonts.Clear();
                FoundFonts.AddRange(fontFamilies);
                FilterFont();
                TextBox.IsEnabled = true;
                ProgressRing.IsVisible = false;
            });
        }, token);
    }

    private void FilterFont()
    {
        FilteredFonts.Clear();
        FilteredFonts.AddRange(FoundFonts.Where(item =>
                item.DisplayName.Replace(" ", "").ToLower().Contains(SearchFunctionText.ToLower().Replace(" ", ""),
                    StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(x => x.FontFamilyName).ToList());
    }
}