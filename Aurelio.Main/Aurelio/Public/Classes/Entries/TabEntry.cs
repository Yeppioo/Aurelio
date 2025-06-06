using System;
using System.Linq;
using Aurelio.Public.Classes.Types;
using Aurelio.ViewModels;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aurelio.Public.Classes.Entries;

public partial class TabEntry : ViewModelBase , IDisposable
{
    public TabEntry(string title, IFunctionPage content, StreamGeometry? icon = null, bool canClose = true,
        object? headerContent = null)
    {
        CanClose = canClose;
        Title = title;
        Icon = icon;
        Content = content;
        HeaderContent = headerContent ?? CreateHeaderTextBlock();
    }

    private IFunctionPage _content;
    private object _headerContent;
    private string _title;
    private StreamGeometry? _icon;
    private bool _canClose;
    public bool IconIsVisible => Icon != null;

    public IFunctionPage Content
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
        if (App.UiRoot.ViewModel.SelectedItem == this)
        {
            App.UiRoot.ViewModel.Tabs.Remove(this);
            App.UiRoot.ViewModel.SelectedItem = App.UiRoot.ViewModel.Tabs.LastOrDefault();
        }
        else
        {
            App.UiRoot.ViewModel.Tabs.Remove(this);
        }
        Content.Dispose();
        Dispose();
    }
    
    public void ReplacePage(IFunctionPage page)
    {
        Content.Dispose();
        Content = page;
        page.HostTab = this;
        Icon = page.GetPageInfo().icon;
        Title = page.GetPageInfo().title;
        Content = page;
    }

    public void Dispose()
    {
        Content.Dispose();
        Content = null;
        GC.SuppressFinalize(this);
        GC.Collect(2);
    }
}