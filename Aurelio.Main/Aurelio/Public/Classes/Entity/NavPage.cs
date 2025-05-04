using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Content;
using Aurelio.Public.Enum;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aurelio.Public.Classes.Entity;

public partial class NavPage : ObservableValidator
{
    public string Id { get; set; }

    public string Header
    {
        get => _header;
        set => SetProperty(ref _header, value);
    }

    public StreamGeometry Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public object Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private string _header;
    private StreamGeometry _icon;
    private NavPageType _type;
    private object _content;
    private bool _isVisible = true;
    public ObservableCollection<NavPage> SubPages { get; set; } = [];

    public NavPage(string id, string header, object content, StreamGeometry icon = null,
        IEnumerable<NavPage>? list = null)
    {
        Id = id;
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
        _header = header;
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
}