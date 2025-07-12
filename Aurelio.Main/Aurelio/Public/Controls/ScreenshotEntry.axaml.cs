using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Module.Service;
using Aurelio.Views.Main;
using Aurelio.Views.Main.Template;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Aurelio.Public.Controls;

public partial class ScreenshotEntry : UserControl, IDisposable
{
    private readonly string _imageName;
    private readonly string _imagePath;
    private bool _disposed;
    private bool _imageLoaded;
    private bool _isVisible;

    public ScreenshotEntry(string name, string path)
    {
        _imageName = name;
        _imagePath = path;
        InitializeComponent();
        FileNameTextBlock.Text = name;

        // ImageBorder 已经有默认背景色，等待图片加载

        // 监听可见性变化
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;

        Root.PointerReleased += OnImageClick;
    }


    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _isVisible = false;

        // 清理事件监听
        AttachedToVisualTree -= OnAttachedToVisualTree;
        DetachedFromVisualTree -= OnDetachedFromVisualTree;
        Root.PointerReleased -= OnImageClick;

        // 清理图片资源
        if (ImageBorder.Background is ImageBrush)
            ImageBorder.Background = new SolidColorBrush(Color.FromArgb(16, 255, 255, 255));
        // 注意：不要在这里 dispose bitmap，因为它可能被缓存复用
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _isVisible = true;
        LoadImageIfNeeded();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _isVisible = false;
    }

    private async void LoadImageIfNeeded()
    {
        if (_imageLoaded || !_isVisible || _disposed) return;

        _imageLoaded = true;

        try
        {
            var bitmap = await ImageCache.GetImageAsync(_imagePath);
            if (bitmap != null && _isVisible && !_disposed)
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (!_disposed)
                    {
                        // 使用 ImageBrush 设置 Border 的背景
                        var imageBrush = new ImageBrush(bitmap)
                        {
                            Stretch = Stretch.UniformToFill
                        };
                        ImageBorder.Background = imageBrush;
                    }
                });
        }
        catch
        {
            // 加载失败时保持默认背景
        }
    }

    private async void OnImageClick(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            // 异步加载高分辨率图片用于查看
            var fullSizeBitmap = await Task.Run(() =>
            {
                using var fileStream = File.OpenRead(_imagePath);
                return Bitmap.DecodeToWidth(fileStream, 1080);
            });

            var tab = new TabEntry(new ImageViewer(_imageName, fullSizeBitmap, _imagePath));

            if (this.GetVisualRoot() is TabWindow window)
            {
                window.CreateTab(tab);
                return;
            }

            App.UiRoot.CreateTab(tab);
        }
        catch
        {
            // 处理加载失败的情况
        }
    }
}