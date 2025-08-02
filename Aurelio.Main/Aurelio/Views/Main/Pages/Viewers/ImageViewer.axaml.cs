using System.Collections.Generic;
using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;

namespace Aurelio.Views.Main.Pages.Viewers;

public partial class ImageViewer : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    private readonly string? _path;

    public ImageViewer(string title, Bitmap image, string? path = null)
    {
        _path = path;
        InitializeComponent();
        PageInfo = new PageInfoEntry
        {
            Title = title,
            Icon = StreamGeometry.Parse(
                "M0 96C0 60.7 28.7 32 64 32l384 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zM323.8 202.5c-4.5-6.6-11.9-10.5-19.8-10.5s-15.4 3.9-19.8 10.5l-87 127.6L170.7 297c-4.6-5.7-11.5-9-18.7-9s-14.2 3.3-18.7 9l-64 80c-5.8 7.2-6.9 17.1-2.9 25.4s12.4 13.6 21.6 13.6l96 0 32 0 208 0c8.9 0 17.1-4.9 21.2-12.8s3.6-17.4-1.4-24.7l-120-176zM112 192a48 48 0 1 0 0-96 48 48 0 1 0 0 96z")
        };
        ShortInfo = path ?? title;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        TitleBlock.Text = path ?? title;
        Viewer.Source = image;
        Loaded += (_, _) =>
        {
            Viewer.TranslateY = 0;
            Viewer.TranslateX = 0;
            Viewer.Scale = 0.6;
        };
        if (Data.DesktopType != DesktopType.Windows) CopyButton.IsVisible = false;
    }
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }
    public ImageViewer()
    {
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    private void OpenFolder(object? sender, RoutedEventArgs e)
    {
        if (_path == null) return;
        _ = Public.Module.Ui.Overlay.OpenFolder(Path.GetDirectoryName(_path)!);
    }

    private void OpenFile(object? sender, RoutedEventArgs e)
    {
        if (_path == null) return;
        var launcher = TopLevel.GetTopLevel(this).Launcher;
        launcher.LaunchFileInfoAsync(new FileInfo(_path));
    }

    private async void Copy(object? sender, RoutedEventArgs e)
    {
        // var clipboard = TopLevel.GetTopLevel(this).Clipboard;
        if (_path == null) return;
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.Clipboard is not { } clipboard || desktop.MainWindow?.StorageProvider is not
                { } storageProvider)
            return;
        var file = await storageProvider.TryGetFileFromPathAsync(_path);
        if (file != null)
        {
            DataObject dataObject = new();
            dataObject.Set(DataFormats.Files, new List<IStorageFile> { file });
            await clipboard.SetDataObjectAsync(dataObject);
        }

        // var obj = new DataObject();
        // using var ms = new MemoryStream();
        // new Bitmap(_path).Save(ms);
        // obj.Set("image/png", ms);
        // clipboard.SetDataObjectAsync(obj);
    }

    private async void DelFile(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_path)) return;

        var title = Data.DesktopType == DesktopType.Windows
            ? MainLang.MoveToRecycleBin
            : MainLang.DeleteSelect;
        var dialog = await ShowDialogAsync(title, $"• {Path.GetFileName(_path)}\n", b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok, sender: sender as Control);
        if (dialog != ContentDialogResult.Primary) return;

        if (Data.DesktopType == DesktopType.Windows)
            FileSystem.DeleteFile(_path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
        else
            File.Delete(_path);
    }
    public static IAurelioNavPage Create((object sender, object? param)t)
    {
        using var fileStream = File.OpenRead((string)t. param!);
        return new ImageViewer(Path.GetFileName((string)t. param!), new Bitmap(fileStream), (string)t. param!);
    }
    
    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse("M0 96C0 60.7 28.7 32 64 32l384 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zM323.8 202.5c-4.5-6.6-11.9-10.5-19.8-10.5s-15.4 3.9-19.8 10.5l-87 127.6L170.7 297c-4.6-5.7-11.5-9-18.7-9s-14.2 3.3-18.7 9l-64 80c-5.8 7.2-6.9 17.1-2.9 25.4s12.4 13.6 21.6 13.6l96 0 32 0 208 0c8.9 0 17.1-4.9 21.2-12.8s3.6-17.4-1.4-24.7l-120-176zM112 192a48 48 0 1 0 0-96 48 48 0 1 0 0 96z"),
        Title = "图片查看器",
        NeedPath = true,
        AutoCreate = false,
        MustPath = true
    };
}