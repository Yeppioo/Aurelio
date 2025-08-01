using System.Collections.ObjectModel;
using System.Threading;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Aurelio.Views.Main.Pages;

public partial class PluginNugetFetcher : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    private string _searchText = string.Empty;
    private CancellationTokenSource? _searchCancellationTokenSource;

    public PluginNugetFetcher()
    {
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = "NuGet插件搜索",
            Icon = StreamGeometry.Parse("M64 128C64 110.3 78.3 96 96 96L544 96C561.7 96 576 110.3 576 128L576 160C576 177.7 561.7 192 544 192L96 192C78.3 192 64 177.7 64 160L64 128zM96 240L544 240L544 480C544 515.3 515.3 544 480 544L160 544C124.7 544 96 515.3 96 480L96 240zM248 304C234.7 304 224 314.7 224 328C224 341.3 234.7 352 248 352L392 352C405.3 352 416 341.3 416 328C416 314.7 405.3 304 392 304L248 304z")
        };
        DataContext = this;
        BindEvents();
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    /// <summary>
    /// 搜索文本
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    /// <summary>
    /// 搜索结果
    /// </summary>
    public ObservableCollection<NugetSearchResult> SearchResults { get; set; } = [];

    /// <summary>
    /// 是否正在搜索
    /// </summary>
    private bool _isSearching;

    public bool IsSearching
    {
        get => _isSearching;
        set => SetField(ref _isSearching, value);
    }

    /// <summary>
    /// 是否显示无结果消息
    /// </summary>
    private bool _showNoResultsMessage;

    public bool ShowNoResultsMessage
    {
        get => _showNoResultsMessage;
        set => SetField(ref _showNoResultsMessage, value);
    }

    /// <summary>
    /// 绑定事件
    /// </summary>
    private void BindEvents()
    {
        // 搜索框回车事件
        SearchBox.KeyDown += OnSearchBoxKeyDown;

        // 搜索按钮点击事件
        SearchButton.Click += OnSearchButtonClick;

        // 搜索图标点击事件
        SearchIcon.PointerPressed += OnSearchIconClick;
    }

    /// <summary>
    /// 搜索框按键事件
    /// </summary>
    private async void OnSearchBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await PerformSearchAsync();
        }
    }

    /// <summary>
    /// 搜索按钮点击事件
    /// </summary>
    private async void OnSearchButtonClick(object? sender, RoutedEventArgs e)
    {
        await PerformSearchAsync();
    }

    /// <summary>
    /// 搜索图标点击事件
    /// </summary>
    private async void OnSearchIconClick(object? sender, PointerPressedEventArgs e)
    {
        await PerformSearchAsync();
    }

    /// <summary>
    /// 执行搜索
    /// </summary>
    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Notice("请输入搜索关键词", NotificationType.Warning);
            return;
        }

        if (IsSearching)
        {
            return; // 防止重复搜索
        }

        // 取消之前的搜索
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();

        try
        {
            // 设置搜索状态
            IsSearching = true;
            ShowNoResultsMessage = false;

            // 清空之前的搜索结果
            await Dispatcher.UIThread.InvokeAsync(() => { SearchResults.Clear(); });

            Logger.Info($"开始搜索NuGet包: {SearchText}");

            var results = await NugetSearchService.SearchPackagesAsync(SearchText);

            // 检查是否被取消
            if (_searchCancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // 重新填充搜索结果
                SearchResults.Clear();
                foreach (var result in results)
                {
                    // 重置每个结果的状态
                    result.IsInstalling = false;
                    result.IsDownloading = false;
                    result.IsLoadingVersions = false;
                    result.IsExpanded = false;

                    SearchResults.Add(result);
                }

                ShowNoResultsMessage = results.Count == 0;

                if (results.Count > 0)
                {
                    Notice($"找到 {results.Count} 个相关插件包", NotificationType.Success);
                }
                else
                {
                    Notice("未找到相关插件包，请尝试其他关键词");
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Error($"搜索失败: {ex.Message}");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Notice($"搜索失败: {ex.Message}", NotificationType.Error);
                ShowNoResultsMessage = false;
            });
        }
        finally
        {
            // 确保搜索状态被重置
            await Dispatcher.UIThread.InvokeAsync(() => { IsSearching = false; });
        }
    }

    /// <summary>
    /// 版本下拉框打开事件
    /// </summary>
    private async void OnVersionComboBoxOpened(object? sender, EventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.DataContext is NugetSearchResult package)
        {
            // 如果版本列表为空，则加载版本信息
            if (package.AllVersions.Count == 0 && !package.IsLoadingVersions)
            {
                try
                {
                    package.IsLoadingVersions = true;

                    var versions = await NugetSearchService.GetPackageVersionsAsync(package.Id);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        package.AllVersions.Clear();
                        foreach (var version in versions)
                        {
                            package.AllVersions.Add(version);
                        }

                        // 如果当前选择的版本不在列表中，设置为最新版本
                        if (!string.IsNullOrEmpty(package.LatestVersion) &&
                            !package.AllVersions.Contains(package.SelectedVersion))
                        {
                            package.SelectedVersion = package.LatestVersion;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error($"加载版本信息失败: {ex.Message}");
                    Notice($"加载版本信息失败: {ex.Message}", NotificationType.Error);
                }
                finally
                {
                    package.IsLoadingVersions = false;
                }
            }
        }
    }

    /// <summary>
    /// 安装按钮点击事件
    /// </summary>
    private async void OnInstallButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is NugetSearchResult package)
        {
            try
            {
                await NugetSearchService.InstallPackageAsync(package, this);
            }
            catch (Exception ex)
            {
                Logger.Error($"安装插件失败: {ex.Message}");
                Notice($"安装失败: {ex.Message}", NotificationType.Error);
            }
        }
    }

    /// <summary>
    /// 另存为按钮点击事件
    /// </summary>
    private async void OnSaveAsButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is NugetSearchResult package)
        {
            try
            {
                await NugetSearchService.SavePackageAsAsync(package, this);
            }
            catch (Exception ex)
            {
                Logger.Error($"下载插件失败: {ex.Message}");
                Notice($"下载失败: {ex.Message}", NotificationType.Error);
            }
        }
    }

    public void OnClose()
    {
        // 清理资源
        try
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            SearchResults.Clear();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse(
            "M64 128C64 110.3 78.3 96 96 96L544 96C561.7 96 576 110.3 576 128L576 160C576 177.7 561.7 192 544 192L96 192C78.3 192 64 177.7 64 160L64 128zM96 240L544 240L544 480C544 515.3 515.3 544 480 544L160 544C124.7 544 96 515.3 96 480L96 240zM248 304C234.7 304 224 314.7 224 328C224 341.3 234.7 352 248 352L392 352C405.3 352 416 341.3 416 328C416 314.7 405.3 304 392 304L248 304z"),
        Title = "NuGet插件搜索",
        NeedPath = false,
        AutoCreate = true
    };

    public static IAurelioNavPage Create((object sender, object? param)t)
    {
        var root = ((Control)t.sender).GetVisualRoot();
        if (root is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new PluginNugetFetcher()));
            return null;
        }
        App.UiRoot.CreateTab(new TabEntry(new PluginNugetFetcher()));
        return null;
    }
}