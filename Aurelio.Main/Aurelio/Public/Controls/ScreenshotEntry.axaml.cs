using System.Diagnostics;
using System.IO;
using System.Threading;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Module.Service;
using Aurelio.Views.Main;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ImageViewer = Aurelio.Views.Main.Pages.ImageViewer;

namespace Aurelio.Public.Controls;

public partial class ScreenshotEntry : UserControl, IDisposable
{
    private const int DelayTime = 500; // 延迟时间，单位毫秒
    private readonly string _imageName;
    private readonly string _imagePath;
    private readonly object _lockObj = new();
    private bool _disposed;
    private bool _imageLoaded;
    private bool _isVisible;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _unloadCts;

    public ScreenshotEntry(string name, string path)
    {
        _imageName = name;
        _imagePath = path;
        InitializeComponent();
        FileNameTextBlock.Text = name;

        // 监听可见性变化
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;

        Root.PointerReleased += OnImageClick;
    }

    public ScreenshotEntry()
    {
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        lock (_lockObj)
        {
            if (_disposed) return;
            _disposed = true;
            _isVisible = false;

            // 取消所有挂起的操作
            SafelyCancelAndDispose(ref _loadCts);
            SafelyCancelAndDispose(ref _unloadCts);

            // 清理事件监听
            AttachedToVisualTree -= OnAttachedToVisualTree;
            DetachedFromVisualTree -= OnDetachedFromVisualTree;
            Root.PointerReleased -= OnImageClick;

            // 清理图片资源
            UnloadImage();
        }
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        lock (_lockObj)
        {
            if (_disposed) return;
            _isVisible = true;

            // 取消之前的卸载请求
            SafelyCancelAndDispose(ref _unloadCts);

            // 创建延迟加载请求
            _loadCts = ImageCache.RequestDelayedLoad($"load_{_imagePath}", DelayTime, async () =>
            {
                if (_disposed || !_isVisible) return;
                await LoadImageAsync();
            });
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        lock (_lockObj)
        {
            if (_disposed) return;
            _isVisible = false;

            // 取消之前的加载请求
            SafelyCancelAndDispose(ref _loadCts);

            // 创建延迟卸载请求
            _unloadCts = ImageCache.RequestDelayedLoad($"unload_{_imagePath}", DelayTime, async () =>
            {
                if (_disposed || _isVisible) return;
                await Dispatcher.UIThread.InvokeAsync(UnloadImage);
            });
        }
    }

    private void SafelyCancelAndDispose(ref CancellationTokenSource? cts)
    {
        if (cts == null) return;

        var localCts = cts;
        cts = null; // 先置空引用，防止其他线程再次访问

        try
        {
            if (!localCts.IsCancellationRequested)
                localCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 已经被处置，忽略
        }
        catch (Exception ex)
        {
            // 其他异常记录但不抛出
            Debug.WriteLine($"Error cancelling token: {ex.Message}");
        }

        try
        {
            localCts.Dispose();
        }
        catch (Exception ex)
        {
            // 处置时出错，记录但不抛出
            Debug.WriteLine($"Error disposing token: {ex.Message}");
        }
    }

    private async Task LoadImageAsync()
    {
        if (_imageLoaded || !_isVisible || _disposed) return;

        try
        {
            var bitmap = await ImageCache.LoadImageAsync(_imagePath);

            lock (_lockObj)
            {
                if (bitmap != null && _isVisible && !_disposed)
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (!_disposed)
                        {
                            // 使用 ImageBrush 设置 Border 的背景
                            var imageBrush = new ImageBrush(bitmap)
                            {
                                Stretch = Stretch.UniformToFill
                            };
                            ImageBorder.Background = imageBrush;
                            _imageLoaded = true;
                        }
                        else
                        {
                            // 如果控件已被处置，释放bitmap
                            bitmap.Dispose();
                        }
                    });
                else if (bitmap != null)
                    // 如果不可见或已处置，释放bitmap
                    bitmap.Dispose();
            }
        }
        catch
        {
            // 加载失败时保持默认背景
        }
    }

    private void UnloadImage()
    {
        lock (_lockObj)
        {
            if (!_imageLoaded) return;

            // 清理图片资源
            if (ImageBorder.Background is ImageBrush brush && brush.Source is Bitmap bitmap)
            {
                ImageBorder.Background = new SolidColorBrush(Color.FromArgb(16, 255, 255, 255));
                bitmap.Dispose();
            }
            else
            {
                ImageBorder.Background = new SolidColorBrush(Color.FromArgb(16, 255, 255, 255));
            }

            _imageLoaded = false;
        }
    }

    private async void OnImageClick(object? sender, PointerReleasedEventArgs e)
    {
        if (_disposed) return;

        try
        {
            // 异步加载高分辨率图片用于查看
            var fullSizeBitmap = await Task.Run(() =>
            {
                using var fileStream = File.OpenRead(_imagePath);
                return Bitmap.DecodeToWidth(fileStream, 1080);
            });

            if (_disposed)
            {
                fullSizeBitmap?.Dispose();
                return;
            }

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