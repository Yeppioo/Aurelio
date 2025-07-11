using System.Linq;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.ViewModels;
using Aurelio.Views.Main;
using Avalonia.Data;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace Aurelio.Public.Classes.Entries;

public partial class TabEntry : ViewModelBase
{
    private bool _canClose;
    private IAurelioTabPage _content;
    private object _headerContent;
    private StreamGeometry? _icon;

    private string _tag;
    private string _title;

    public TabEntry(IAurelioTabPage content, string? title = null, object? headerContent = null)
    {
        CanClose = content.PageInfo.CanClose;
        Title = title ?? content.PageInfo.Title;
        Icon = content.PageInfo.Icon;
        Content = content;
        HeaderContent = headerContent ?? CreateHeaderTextBlock();
        content.HostTab = this;
    }

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
        textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(Title)));
        return textBlock;
    }

    public void RefreshContent()
    {
        // Force refresh of content to avoid layout manager conflicts
        if (Content?.RootElement != null)
        {
            Content.RootElement.InvalidateVisual();
            Content.RootElement.InvalidateMeasure();
        }
    }

    public void Close()
    {
        if (!CanClose) return;

        var wasSelected = App.UiRoot.ViewModel.SelectedTab == this;
        App.UiRoot.ViewModel.Tabs.Remove(this);

        // If the removed tab was selected, select the last remaining tab (or null if no tabs left)
        if (wasSelected) App.UiRoot.ViewModel.SelectedTab = App.UiRoot.ViewModel.Tabs.LastOrDefault();

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
        GC.Collect(2, GCCollectionMode.Aggressive, true);
    }

    public void DisposeContent()
    {
        Content?.OnClose();
    }

    [RelayCommand]
    public void CloseInWindow(Window window)
    {
        if (!CanClose) return;

        if (window is MainWindow mainWindow)
        {
            Close(); // Use existing Close method for MainWindow
        }
        else if (window is TabWindow tabWindow)
        {
            // Remove from TabWindow - RemoveTab method already handles selection logic
            tabWindow.ViewModel.RemoveTab(this);

            // If TabWindow has no more tabs, close it
            if (!tabWindow.ViewModel.HasTabs) tabWindow.Close();

            DisposeContent();
            Removing();
        }
    }
}