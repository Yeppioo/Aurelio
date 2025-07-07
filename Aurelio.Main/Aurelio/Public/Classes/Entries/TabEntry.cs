using System;
using System.Linq;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.ViewModels;
using Avalonia.Controls;
using Avalonia.Media;

namespace Aurelio.Public.Classes.Entries;

public partial class TabEntry : ViewModelBase
{
    public TabEntry(IAurelioTabPage content, string? title = null, object? headerContent = null)
    {
        CanClose = content.PageInfo.CanClose;
        Title = title ?? content.PageInfo.Title;
        Icon = content.PageInfo.Icon;
        Content = content;
        HeaderContent = headerContent ?? CreateHeaderTextBlock();
        content.HostTab = this;
    }

    private string _tag;
    private IAurelioTabPage _content;
    private object _headerContent;
    private string _title;
    private StreamGeometry? _icon;
    private bool _canClose;
    public bool IconIsVisible => Icon != null;

    public IAurelioTabPage Content
    {
        get => _content;
        set => SetField(ref _content, value);
    }

    public StreamGeometry? Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Tag
    {
        get => _tag;
        set => SetField(ref _tag, value);
    }

    public bool CanClose
    {
        get => _canClose;
        set => SetField(ref _canClose, value);
    }

    public object HeaderContent
    {
        get => _headerContent;
        set => SetField(ref _headerContent, value);
    }

    private TextBlock CreateHeaderTextBlock()
    {
        var textBlock = new TextBlock
        {
            DataContext = this
        };
        textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(Title)));
        return textBlock;
    }

    public void Close()
    {
        if (!CanClose) return;
        if (App.UiRoot.ViewModel.SelectedTab == this)
        {
            App.UiRoot.ViewModel.Tabs.Remove(this);
            App.UiRoot.ViewModel.SelectedTab = App.UiRoot.ViewModel.Tabs.LastOrDefault();
        }
        else
        {
            App.UiRoot.ViewModel.Tabs.Remove(this);
        }

        DisposeContent();
        Removing();
    }

    public void ReplacePage(IAurelioTabPage tabPage)
    {
        DisposeContent();
        Content = tabPage;
        tabPage.HostTab = this;
        Icon = tabPage.PageInfo.Icon;
        Title = tabPage.PageInfo.Title;
        Content = tabPage;
    }

    public void Removing()
    {
        DisposeContent();
        Content = null;
        GC.SuppressFinalize(this);
        GC.Collect(2);
    }

    public void DisposeContent() => Content?.OnClose();
}