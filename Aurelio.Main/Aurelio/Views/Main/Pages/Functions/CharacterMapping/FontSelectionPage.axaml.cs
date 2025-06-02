using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Const;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DynamicData;
using SkiaSharp;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Threading;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontSelectionPage : UserControl, IFunctionPage, INotifyPropertyChanged, IDisposable
{
    private bool _fl = true;
    private CancellationTokenSource _fontLoadCts;

    public FontSelectionPage()
    {
        InitializeComponent();
        DataContext = this;
        ListBox.SelectionChanged += (_, _) =>
        {
            if (ListBox.SelectedItem is not RecordFontFamilyEntry item) return;
            IFunctionPage page = new FontMappingTablePage(item);
            var text = new TextBlock();
            text.Inlines.Add(new Run($"{MainLang.CharacterMapping}: ") { FontFamily = item.FontFamily });
            text.Inlines.Add(new Run(item.DisplayName) { FontFamily = item.FontFamily });
            page.HostContent = (FontMappingTablePage)page;
            page.HostTab = HostTab;
            page.HostTab.Icon = page.GetPageInfo().icon;
            page.HostTab.Title = page.GetPageInfo().title;
            page.HostTab.HeaderContent = text;
            page.HostTab.Content = page.HostContent;
            Dispose();
        };
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            _fl = false;
            GetFonts();
            FilterFont();
        };
    }

    public (string title, StreamGeometry icon, Action OnClose) GetPageInfo()
    {
        return ($"{MainLang.CharacterMapping}: {MainLang.FontList}", Icons.CharacterAppearance, OnClose);
    }

    public TabEntry HostTab { get; set; }
    public UserControl HostContent { get; set; }

    public void OnClose()
    {
        Dispose();
    }

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
                    fontDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\\Windows\\Fonts"));
                    break;
                case DesktopType.MacOs:
                    fontDirs.Add("/System/Library/Fonts");
                    fontDirs.Add("/Library/Fonts");
                    fontDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts"));
                    break;
                case DesktopType.Linux:
                case DesktopType.FreeBSD:
                    fontDirs.Add("/usr/share/fonts");
                    fontDirs.Add("/usr/local/share/fonts");
                    fontDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"));
                    break;
                case DesktopType.Unknown:
                default:
                    break;
            }

            string[] fontExtensions = { "*.ttf", "*.otf", "*.ttc", "*.woff", "*.woff2" };
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

            // 第一遍：收集所有家族名
            var familyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in fontFiles.Distinct())
            {
                try
                {
                    using var skTypeface = SKTypeface.FromFile(file);
                    if (skTypeface == null) continue;
                    var familyName = skTypeface.FamilyName;
                    if (!string.IsNullOrWhiteSpace(familyName))
                        familyNames.Add(familyName);
                }
                catch
                {
                    // ignored
                }
            }

            // 第二遍：为每个家族收集所有文件及其style/weight
            var fontFamilyDict = new Dictionary<string, RecordFontFamilyEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var familyName in familyNames)
            {
                var entry = new RecordFontFamilyEntry
                {
                    FontFamily = new FontFamily(familyName),
                    DisplayName = FontFamilyChineseNameTable.FontNameMap.TryGetValue(familyName, out var cn) && !string.IsNullOrWhiteSpace(cn)
                        ? $"{familyName} ({cn})"
                        : familyName,
                    FontFamilyName = familyName,
                    Styles = "",
                    Typefaces = new List<RecordTypefaceEntry>()
                };

                foreach (var file in fontFiles.Distinct())
                {
                    try
                    {
                        using var skTypeface = SKTypeface.FromFile(file);
                        if (skTypeface == null) continue;
                        if (!string.Equals(skTypeface.FamilyName, familyName, StringComparison.OrdinalIgnoreCase)) continue;

                        var weight = skTypeface.FontWeight;
                        var isItalic = skTypeface.IsItalic;
                        string styleName = isItalic ? "Italic" : "Normal";
                        string weightName = weight == 400 ? "" : weight.ToString();
                        string name = string.IsNullOrEmpty(weightName) ? styleName : $"{weightName} {styleName}";

                        // 避免重复
                        if (entry.Typefaces.Any(x => x.Name == name && x.Path == file)) continue;

                        entry.Typefaces.Add(new RecordTypefaceEntry
                        {
                            Name = name,
                            Typeface = null,
                            Path = file
                        });
                    }
                    catch
                    {
                        // ignored
                    }
                }

                entry.Styles = string.Join(",", entry.Typefaces.Select(x => x.Name).Distinct());
                fontFamilyDict[familyName] = entry;
            }

            var fontFamilies = fontFamilyDict.Values.ToList();

            if (!token.IsCancellationRequested)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    FoundFonts.Clear();
                    FoundFonts.AddRange(fontFamilies);
                    FilterFont();
                    TextBox.IsEnabled = true;
                    ProgressRing.IsVisible = false;
                });
            }
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

    public new event PropertyChangedEventHandler? PropertyChanged;

    private new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    public void Dispose()
    {
        try
        {
            FoundFonts.Clear();
            _fontLoadCts?.Cancel();
            _fontLoadCts?.Dispose();
            GC.Collect(2, GCCollectionMode.Aggressive, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}