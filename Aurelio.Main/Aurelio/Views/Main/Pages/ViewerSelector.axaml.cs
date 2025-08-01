using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.Viewers;
using Aurelio.Views.Main.Pages.Viewers.Terminal;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace Aurelio.Views.Main.Pages;

public class ViewerSelectorEntry(AurelioViewerInfo viewerInfo, Func<string, object> action)
{
    public AurelioViewerInfo ViewerInfo { get; set; } = viewerInfo;
    public Func<string, object> Create = action;
}

public partial class ViewerSelector : PageMixModelBase, IAurelioTabPage, INotifyPropertyChanged
{
    private double _containerWidth = 800;

    public ViewerSelector()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Icon = StreamGeometry.Parse(
                "M541.9 139.5C546.4 127.7 543.6 114.3 534.7 105.4C525.8 96.5 512.4 93.6 500.6 98.2L84.6 258.2C71.9 263 63.7 275.2 64 288.7C64.3 302.2 73.1 314.1 85.9 318.3L262.7 377.2L321.6 554C325.9 566.8 337.7 575.6 351.2 575.9C364.7 576.2 376.9 568 381.8 555.4L541.8 139.4z"),
            Title = MainLang.OpenFile
        };

        // 监听窗口大小变化
        Root.PropertyChanged += (sender, e) =>
        {
            if (e.Property == Visual.BoundsProperty)
            {
                var newWidth = Root?.Bounds.Width ?? 800;
                if (Math.Abs(_containerWidth - newWidth) > 1)
                {
                    _containerWidth = newWidth;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContainerWidth)));
                }
            }
        };
    }

    public ObservableCollection<ViewerSelectorEntry> Viewers { get; set; } =
    [
        new(CodeViewer.ViewerInfo, CodeViewer.Create),
        new(TerminalViewer.ViewerInfo, TerminalViewer.Create),
        new(ImageViewer.ViewerInfo, ImageViewer.Create),
        new(LogViewer.ViewerInfo, LogViewer.Create),
        new(JsonViewer.ViewerInfo, JsonViewer.Create),
        new(ZipViewer.ViewerInfo, ZipViewer.Create)
    ];

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public double ContainerWidth => _containerWidth;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public void OnClose()
    {
    }

    private async void ViewerCardBorder_OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is not Border { Tag: ViewerSelectorEntry entry }) return;
        var fs = await this.PickFileAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            Title = MainLang.OpenFile,
        });
        if (this.GetVisualRoot() is TabWindow window)
        {
            foreach (var f in fs)
            {
                window.TogglePage(null, (entry.Create(f) as IAurelioTabPage)!);
            }
        }
        else
        {
            foreach (var f in fs)
            {
                App.UiRoot.TogglePage(null, (entry.Create(f) as IAurelioTabPage)!);
            }
        }
    }
}