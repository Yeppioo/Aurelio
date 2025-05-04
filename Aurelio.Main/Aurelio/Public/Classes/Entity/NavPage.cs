using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Aurelio.Public.Content;
using Aurelio.Public.Enum;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aurelio.Public.Classes.Entity;

public partial class NavPage : ObservableValidator, INotifyPropertyChanged
{
    public string Id { get; set; }

    public string Header
    {
        get => _header;
        set => SetField(ref _header, value);
    }

    public string Summary
    {
        get => _summary;
        set => SetField(ref _summary, value);
    }

    public StreamGeometry Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public object Content
    {
        get => _content;
        set => SetField(ref _content, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    private string _header;
    private string _summary;
    private StreamGeometry _icon;
    private NavPageType _type;
    private object? _content;
    private bool _isExpanded;
    public ObservableCollection<NavPage> SubPages { get; set; } = [];

    public NavPage(string id, string header, object content, StreamGeometry icon = null,
        string summary = null, IEnumerable<NavPage>? list = null)
    {
        Id = id;
        _content = content;
        _header = header;
        _summary = summary ?? "ヾ(•ω•`)o";
        _icon = icon == null ? Icons.Favicon : icon;
        if (list == null || !list.Any()) return;
        foreach (var navPageBase in list)
        {
            SubPages.Add(navPageBase);
        }
    }

    public NavPage FindById(string id)
    {
        foreach (var subPage in SubPages)
        {
            var found = subPage.FindById(id);
            if (found != null) return found;
        }

        if (Id == id) return this;
        return null;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public static class NavPageExtensions
{
    public static NavPage FindById(this IEnumerable<NavPage> navPages, string id)
    {
        foreach (var navPage in navPages)
        {
            var found = navPage.FindById(id);
            if (found != null) return found;
        }

        return null;
    }

    public static ObservableCollection<NavPage> Instance(this ObservableCollection<NavPage> navPages)
    {
        return navPages;
    }
}