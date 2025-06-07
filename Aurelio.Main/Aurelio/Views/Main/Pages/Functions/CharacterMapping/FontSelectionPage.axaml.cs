using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
using Aurelio.Public.Classes.Types;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontSelectionPage : UserControl, IFunctionPage {
    private CancellationTokenSource _fontLoadCts;
    private string _searchFunctionText = string.Empty;

    public TabEntry HostTab { get; set; }
    public UserControl HostContent { get; set; }
    public ObservableCollection<FontFamily> FoundFonts { get; set; } = [];
    public ObservableCollection<FontFamily> FilteredFonts { get; set; } = [];

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string SearchFunctionText {
        get => _searchFunctionText;
        set {
            SetField(ref _searchFunctionText, value);
            FilterFont();
        }
    }

    public FontSelectionPage() {
        InitializeComponent();
        DataContext = this;
        ListBox.SelectionChanged += OnSelectionChanged;
    }

    public void Dispose() {
        FoundFonts?.Clear();
        _fontLoadCts?.Dispose();
        GC.SuppressFinalize(this);
    }

    public (string title, StreamGeometry icon) GetPageInfo() {
        return ($"{MainLang.CharacterMapping}: {MainLang.FontList}", Icons.CharacterAppearance);
    }

    private void GetFonts() {
        FoundFonts.Clear();
        FoundFonts = [.. FontManager.Current.SystemFonts.Select(x => x)];

        if (FoundFonts.Count > 0) {
            TextBox.IsEnabled = true;
            ProgressRing.IsVisible = false;
        }
    }

    private void FilterFont() {
        FilteredFonts.Clear();
        string searchText = SearchFunctionText.Trim();
        var filteredList = FoundFonts
            .Where(item => item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        FilteredFonts.AddRange(filteredList);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (ListBox.SelectedItem is not FontFamily fontFamily)
            return;

        var page = new FontMappingTablePage(new());
        var text = new TextBlock();
        text.Inlines.Add(new Run($"{MainLang.CharacterMapping}: ") { FontFamily = fontFamily });
        text.Inlines.Add(new Run(fontFamily.Name) { FontFamily = fontFamily });
        HostTab.ReplacePage(page);
        Dispose();
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

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        GetFonts();
        FilteredFonts.AddRange(FoundFonts);
    }
}