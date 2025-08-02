using System.Collections.ObjectModel;
using System.Diagnostics;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Controls;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Plugin.Minecraft.Views.MinecraftInstancePages;

public partial class ScreenshotPage : PageMixModelBase, IAurelioPage
{
    private const int BatchSize = 50; // 每批加载的数量
    private readonly ObservableCollection<MinecraftLocalResourcePackEntry> _allItems = []; // 所有项目
    private readonly List<ScreenshotEntry> _createdEntries = new();
    private readonly MinecraftEntry _entry;
    private readonly ObservableCollection<MinecraftLocalResourcePackEntry> _filteredItems = []; // 过滤后的项目
    private List<string> _allFiles = [];
    private int _currentBatch;
    private string _filter = string.Empty;
    private bool _fl = true;
    private bool _isLoadingBatch; // 防止重复加载
    private bool loading = true;
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }

    public ScreenshotPage(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Filter)) FilterItems();
        };
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            _fl = false;
            LoadItems();
        };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadItems(); };
        OpenFolderBtn.Click += (_, _) =>
        {
            var path = Calculator.GetMinecraftSpecialFolder(_entry,
                MinecraftSpecialFolder.ScreenshotsFolder);
            Setter.TryCreateFolder(path);
            _ = OpenFolder(path);
        };
        ShortInfo = $"{_entry.Id} / {MainLang.Screenshots}";
        // 添加滚动事件监听，实现无限滚动
        Loaded += (_, _) =>
        {
            // 找到 ScrollViewer 并监听滚动事件
            if (this.FindControl<ScrollViewer>("ScrollViewer") is ScrollViewer scrollViewer)
                scrollViewer.PropertyChanged += OnScrollChanged;
        };
    }

    public ScreenshotPage()
    {
    }

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public bool Loading
    {
        get => loading;
        set => SetField(ref loading, value);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }

    /// <summary>
    ///     清理资源，释放所有图片内存
    /// </summary>
    public void OnClose()
    {
        // 清理所有挂起的加载任务
        ImageCache.ClearPendingTasks();

        // 强制调用GC
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // 清理所有 ScreenshotEntry 控件
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                foreach (var child in Enumerable.OfType<ScreenshotEntry>(Container.Children))
                    try
                    {
                        child.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error disposing screenshot entry: {ex.Message}");
                    }

                Container.Children.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing container: {ex.Message}");
            }
        });

        // 清理集合
        _allItems.Clear();
        _filteredItems.Clear();
        _allFiles.Clear();
        _createdEntries.Clear();
    }

    private void LoadItems()
    {
        Loading = true;
        _currentBatch = 0;
        _isLoadingBatch = false;

        Task.Run(() =>
        {
            _allItems.Clear();
            _filteredItems.Clear();

            // 获取所有文件并按时间排序（最新的在前）
            _allFiles = Directory.GetFiles(
                    Calculator.GetMinecraftSpecialFolder(_entry,
                        MinecraftSpecialFolder.ScreenshotsFolder)
                    , "*.png", SearchOption.AllDirectories)
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .ToList();

            // 创建所有项目但不立即显示
            foreach (var file in _allFiles)
                _allItems.Add(new MinecraftLocalResourcePackEntry
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    Icon = null,
                    Description = $"{MainLang.ImportTime}: {new FileInfo(file).CreationTime}"
                });

            ShortInfo = $"{_entry.Id} / {MainLang.Screenshots} / 已加载 {_allFiles.Count} 个截图";
            // 应用当前过滤器并加载第一批
            FilterItems();
            Loading = false;
        });
    }

    private void LoadNextBatch()
    {
        if (_isLoadingBatch) return; // 防止重复加载

        var startIndex = _currentBatch * BatchSize;
        var endIndex = Math.Min(startIndex + BatchSize, _filteredItems.Count);

        if (startIndex >= _filteredItems.Count) return;

        _isLoadingBatch = true;
        var batchItems = _filteredItems.Skip(startIndex).Take(BatchSize);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var item in batchItems)
            {
                var entry = new ScreenshotEntry(item.Name, item.Path);
                Container.Children.Add(entry);
                _createdEntries.Add(entry);
            }

            NoMatchResultTip.IsVisible = Container.Children.Count == 0;

            _currentBatch++;
            _isLoadingBatch = false; // 加载完成，重置标志
        });
    }

    private void OnScrollChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // 检查是否滚动到底部，如果是则加载更多
        if (sender is ScrollViewer scrollViewer && e.Property.Name == "Offset")
        {
            var scrollableHeight = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;
            var isNearBottom = scrollViewer.Offset.Y >= scrollableHeight - 200;

            // 添加更多条件防止重复触发
            if (isNearBottom &&
                !Loading &&
                !_isLoadingBatch &&
                _currentBatch * BatchSize < _filteredItems.Count &&
                scrollableHeight > 0) // 确保有可滚动内容
                LoadNextBatch();
        }
    }

    private void FilterItems()
    {
        // 清理当前所有控件
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var child in Enumerable.OfType<ScreenshotEntry>(Container.Children))
                child.Dispose();

            Container.Children.Clear();
            _createdEntries.Clear();
        });

        // 重新过滤所有项目
        _filteredItems.Clear();
        var filteredItems = _allItems.Where(item
            => item.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var item in filteredItems) _filteredItems.Add(item);

        // 重置批次计数器和加载标志
        _currentBatch = 0;
        _isLoadingBatch = false;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            NoMatchResultTip.IsVisible = _filteredItems.Count == 0;

            // 加载第一批
            if (_filteredItems.Count > 0) LoadNextBatch();
        });
    }
}