using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Http;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Downloader;
using FluentAvalonia.UI.Controls;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace Aurelio.Public.Module.Service;

public static class NugetSearchService
{
    private const string NUGET_SEARCH_API = "https://azuresearch-usnc.nuget.org/query";
    private const string NUGET_VERSIONS_API = "https://api.nuget.org/v3-flatcontainer";
    private const string NUGET_DOWNLOAD_BASE = "https://api.nuget.org/v3-flatcontainer";

    /// <summary>
    /// 搜索NuGet包
    /// </summary>
    /// <param name="searchTerm">搜索关键词</param>
    /// <param name="take">返回结果数量</param>
    /// <param name="includePrerelease">是否包含预发布版本</param>
    /// <returns>搜索结果列表</returns>
    public static async System.Threading.Tasks.Task<List<NugetSearchResult>> SearchPackagesAsync(string searchTerm, int take = 20, bool includePrerelease = false)
    {
        try
        {
            Logger.Info($"搜索NuGet包: {searchTerm}");

            var url = $"{NUGET_SEARCH_API}?q={Uri.EscapeDataString(searchTerm)}&take={take}&prerelease={includePrerelease.ToString().ToLower()}";
            
            var response = await url
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithHeader("User-Agent", "Aurelio-App")
                .GetStringAsync();

            var searchResult = JObject.Parse(response);
            var data = searchResult["data"]?.ToObject<JArray>();

            if (data == null)
            {
                Logger.Warning("NuGet搜索返回空结果");
                return new List<NugetSearchResult>();
            }

            var results = new List<NugetSearchResult>();

            foreach (var item in data)
            {
                try
                {
                    var result = new NugetSearchResult
                    {
                        Id = item["id"]?.ToString() ?? string.Empty,
                        Title = item["title"]?.ToString() ?? item["id"]?.ToString() ?? string.Empty,
                        Description = item["description"]?.ToString() ?? string.Empty,
                        Authors = item["authors"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                        TotalDownloads = item["totalDownloads"]?.ToObject<long>() ?? 0,
                        LatestVersion = item["version"]?.ToString() ?? string.Empty,
                        IconUrl = item["iconUrl"]?.ToString() ?? string.Empty,
                        ProjectUrl = item["projectUrl"]?.ToString() ?? string.Empty,
                        LicenseUrl = item["licenseUrl"]?.ToString() ?? string.Empty,
                        Tags = item["tags"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                        Published = item["published"]?.ToObject<DateTime?>()
                    };

                    // 设置默认选择版本为最新版本
                    result.SelectedVersion = result.LatestVersion;

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    Logger.Error($"解析搜索结果项时出错: {ex.Message}");
                }
            }

            Logger.Info($"搜索完成，找到 {results.Count} 个包");
            return results;
        }
        catch (Exception ex)
        {
            Logger.Error($"搜索NuGet包时发生错误: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取包的所有版本
    /// </summary>
    /// <param name="packageId">包ID</param>
    /// <returns>版本列表</returns>
    public static async System.Threading.Tasks.Task<List<string>> GetPackageVersionsAsync(string packageId)
    {
        try
        {
            var versionsUrl = $"{NUGET_VERSIONS_API}/{packageId.ToLowerInvariant()}/index.json";
            var response = await versionsUrl
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithHeader("User-Agent", "Aurelio-App")
                .GetStringAsync();

            var versionsJson = JObject.Parse(response);
            var versions = versionsJson["versions"]?.ToObject<string[]>();

            if (versions == null || versions.Length == 0)
            {
                Logger.Warning($"包 {packageId} 没有找到版本信息");
                return new List<string>();
            }

            // 按版本号排序，最新版本在前
            var sortedVersions = versions
                .Where(v => Version.TryParse(v, out _))
                .OrderByDescending(v => Version.Parse(v))
                .ToList();

            return sortedVersions;
        }
        catch (Exception ex)
        {
            Logger.Error($"获取包 {packageId} 版本信息时发生错误: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// 安装NuGet包到插件目录
    /// </summary>
    /// <param name="package">包信息</param>
    /// <param name="sender">发送者控件</param>
    public static async Task InstallPackageAsync(NugetSearchResult package, Control? sender = null)
    {
        // 如果没有选择版本，使用最新版本
        if (string.IsNullOrEmpty(package.SelectedVersion))
        {
            package.SelectedVersion = package.LatestVersion;
        }

        if (string.IsNullOrEmpty(package.SelectedVersion))
        {
            Notice("无法获取包版本信息", NotificationType.Warning);
            return;
        }

        var hostId = sender != null ? GetHostId(sender) : "MainWindow";

        try
        {
            // 设置安装状态
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                package.IsInstalling = true;
                package.IsDownloading = false; // 确保下载状态被重置
            });

            // 创建主任务
            var task = Tasking.CreateTask($"安装 {package.Id}");
            var downloadSubTask = new TaskEntry($"下载 {package.Id}");
            var installSubTask = new TaskEntry($"安装 {package.Id}");

            downloadSubTask.AddIn(task);
            installSubTask.AddIn(task);

            task.ProgressIsIndeterminate = false;
            task.ButtonText = MainLang.Cancel;
            task.IsButtonEnable = true;
            task.TaskState = TaskState.Running;

            var cts = new CancellationTokenSource();
            task.ButtonAction = () =>
            {
                task.CancelWaitFinish();
                cts.Cancel();
            };

            // 打开任务抽屉
            _ = OpenTaskDrawer(hostId);

            // 开始下载任务
            task.NextSubTask();

            // 构建下载URL和文件路径（去除版本号）
            var downloadUrl = $"{NUGET_DOWNLOAD_BASE}/{package.Id.ToLowerInvariant()}/{package.SelectedVersion}/{package.Id.ToLowerInvariant()}.{package.SelectedVersion}.nupkg";
            var fileName = $"{package.Id}.nupkg"; // 去除版本号
            var targetFilePath = Path.Combine(ConfigPath.PluginFolderPath, fileName);

            // 检查文件是否已存在
            if (File.Exists(targetFilePath))
            {
                var result = await ShowDialogAsync(
                    title: "插件已存在",
                    msg: $"插件 {package.Id} 已经存在，安装新版本 v{package.SelectedVersion} 将覆盖现有版本。是否继续？",
                    b_primary: "覆盖安装",
                    b_cancel: "取消",
                    sender: sender
                );

                if (result != ContentDialogResult.Primary)
                {
                    task.CancelFinish();
                    await Dispatcher.UIThread.InvokeAsync(() => package.IsInstalling = false);
                    return;
                }
            }

            await DownloadPackageAsync(downloadUrl, targetFilePath, downloadSubTask, cts.Token);

            if (cts.Token.IsCancellationRequested)
            {
                task.CancelFinish();
                await Dispatcher.UIThread.InvokeAsync(() => package.IsInstalling = false);
                return;
            }

            // 开始安装任务
            task.NextSubTask();
            installSubTask.TaskState = TaskState.Running;
            installSubTask.ProgressIsIndeterminate = true;

            await Task.Delay(500, cts.Token); // 模拟安装过程

            installSubTask.FinishWithSuccess();
            task.FinishWithSuccess();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                package.IsInstalling = false;
                // 显示重启提示
                ShowInstallRestartPrompt(package.Id);
            });

            Logger.Info($"成功安装插件: {package.Id} v{package.SelectedVersion}");
        }
        catch (Exception ex)
        {
            Logger.Error($"安装插件 {package.Id} 时发生错误: {ex.Message}");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                package.IsInstalling = false;
                Notice($"安装失败: {ex.Message}", NotificationType.Error);
            });
        }
    }

    /// <summary>
    /// 下载包文件到指定位置
    /// </summary>
    /// <param name="package">包信息</param>
    /// <param name="sender">发送者控件</param>
    public static async Task SavePackageAsAsync(NugetSearchResult package, Control? sender = null)
    {
        // 如果没有选择版本，使用最新版本
        if (string.IsNullOrEmpty(package.SelectedVersion))
        {
            package.SelectedVersion = package.LatestVersion;
        }

        if (string.IsNullOrEmpty(package.SelectedVersion))
        {
            Notice("无法获取包版本信息", NotificationType.Warning);
            return;
        }

        try
        {
            // 选择保存位置
            var fileName = $"{package.Id}.{package.SelectedVersion}.nupkg";
            var savePath = await sender!.PickSaveFileAsync(new FilePickerSaveOptions
            {
                Title = "保存NuGet包",
                SuggestedFileName = fileName,
                DefaultExtension = "nupkg"
            });

            if (string.IsNullOrEmpty(savePath))
            {
                return; // 用户取消了保存
            }

            // 设置下载状态
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                package.IsDownloading = true;
                package.IsInstalling = false; // 确保安装状态被重置
            });

            // 创建下载任务
            var task = Tasking.CreateTask($"下载 {package.Id}");
            var downloadSubTask = new TaskEntry($"下载 {package.Id}");
            downloadSubTask.AddIn(task);

            task.ProgressIsIndeterminate = false;
            task.ButtonText = MainLang.Cancel;
            task.IsButtonEnable = true;
            task.TaskState = TaskState.Running;

            var cts = new CancellationTokenSource();
            task.ButtonAction = () =>
            {
                task.CancelWaitFinish();
                cts.Cancel();
            };

            // 开始下载
            task.NextSubTask();

            var downloadUrl = $"{NUGET_DOWNLOAD_BASE}/{package.Id.ToLowerInvariant()}/{package.SelectedVersion}/{package.Id.ToLowerInvariant()}.{package.SelectedVersion}.nupkg";

            await DownloadPackageAsync(downloadUrl, savePath, downloadSubTask, cts.Token);

            if (!cts.Token.IsCancellationRequested)
            {
                task.FinishWithSuccess();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    package.IsDownloading = false;
                    Notice($"包 {package.Id} v{package.SelectedVersion} 已保存到 {savePath}", NotificationType.Success);
                });
            }
            else
            {
                task.CancelFinish();
                await Dispatcher.UIThread.InvokeAsync(() => package.IsDownloading = false);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"下载包 {package.Id} 时发生错误: {ex.Message}");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                package.IsDownloading = false;
                Notice($"下载失败: {ex.Message}", NotificationType.Error);
            });
        }
    }

    /// <summary>
    /// 下载包文件的通用方法
    /// </summary>
    private static async Task DownloadPackageAsync(string downloadUrl, string targetPath, TaskEntry downloadTask, CancellationToken cancellationToken)
    {
        var downloadConfig = new DownloadConfiguration()
        {
            ChunkCount = 5,
            ParallelDownload = true
        };

        var downloader = new DownloadService(downloadConfig);

        // 设置下载进度回调
        downloader.DownloadProgressChanged += (_, e) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var bytesReceived = e.ReceivedBytesSize.CalcMemoryMensurableUnit();
                var totalBytes = e.TotalBytesToReceive.CalcMemoryMensurableUnit();
                var speed = e.BytesPerSecondSpeed.CalcMemoryMensurableUnit();

                downloadTask.ProgressValue = Math.Round(e.ProgressPercentage, 2);
                downloadTask.ProgressIsIndeterminate = false;
                downloadTask.TopRightInfoText = $"{speed}/s";
                downloadTask.BottomLeftInfoText = $"{bytesReceived}/{totalBytes}";
            });
        };

        var downloadCompleted = false;
        var downloadException = (Exception?)null;

        downloader.DownloadFileCompleted += (_, args) =>
        {
            downloadCompleted = true;
            downloadException = args.Error;
        };

        // 开始下载
        await downloader.DownloadFileTaskAsync(downloadUrl, targetPath, cancellationToken);

        // 等待下载完成
        while (!downloadCompleted && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
        }

        if (downloadException != null)
        {
            throw downloadException;
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            downloadTask.FinishWithSuccess();
        }
    }

    /// <summary>
    /// 显示安装完成后的重启提示对话框
    /// </summary>
    private static async void ShowInstallRestartPrompt(string pluginName)
    {
        try
        {
            var result = await ShowDialogAsync(
                title: MainLang.NeedRestartApp,
                msg: $"{pluginName} 安装完成！为了使插件生效，需要重启 Aurelio。是否现在重启？",
                b_primary: MainLang.RestartNow,
                b_secondary: MainLang.RestartLater,
                b_cancel: MainLang.Cancel
            );

            switch (result)
            {
                case ContentDialogResult.Primary:
                    // 立即重启
                    Logger.Info($"用户选择立即重启应用以应用 {pluginName} 的安装");
                    AppMethod.RestartApp();
                    break;

                case ContentDialogResult.Secondary:
                    // 稍后重启，显示通知
                    Notice($"{pluginName} 安装完成，请稍后重启 Aurelio 以应用更改", NotificationType.Success,
                        TimeSpan.FromSeconds(5));
                    Logger.Info($"用户选择稍后重启应用以应用 {pluginName} 的安装");
                    break;

                default:
                    // 取消，只显示成功通知
                    Notice($"{pluginName} 安装完成", NotificationType.Success);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"显示重启提示时发生错误: {ex.Message}");
            // 如果对话框失败，至少显示通知
            Notice($"{pluginName} 安装完成，请重启 Aurelio 以应用更改", NotificationType.Success);
        }
    }
}
