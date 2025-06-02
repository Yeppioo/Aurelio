using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Functions;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aurelio.Views.Main.Pages.Functions.CharacterMapping;

public partial class FontMappingTablePage : UserControl, IFunctionPage, INotifyPropertyChanged
{
    private bool _fl = true;
    public RecordFontFamilyEntry Entry { get; }
    public FontMappingTablePage(RecordFontFamilyEntry entry)
    {
        Entry = entry;
        InitializeComponent();
        DataContext = this;
        SelectedTypeface = (RecordTypefaceEntry)Entry.Typefaces.FirstOrDefault();
        Loaded += (_, _) =>
        {
            if(!_fl) return;
            _fl = false;
            
        };
    }

    public (string title, StreamGeometry icon, Action OnClose) GetPageInfo()
    {
        return ($"{MainLang.CharacterMapping}: {Entry.DisplayName}", Icons.CharacterAppearance, OnClose);
    }

    public TabEntry HostTab { get; set; }
    public UserControl HostContent { get; set; }

    public void OnClose()
    {
    }

    public RecordTypefaceEntry SelectedTypeface { get; }


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
}