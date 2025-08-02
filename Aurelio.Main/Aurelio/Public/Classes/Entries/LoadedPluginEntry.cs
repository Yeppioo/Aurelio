using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Aurelio.Plugin.Base;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Http;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Downloader;
using FluentAvalonia.UI.Controls;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.IO.Compression;
using System.Threading;
using Setter = Aurelio.Public.Module.IO.Local.Setter;

namespace Aurelio.Public.Classes.Entries;

public class LoadedPluginEntry : ReactiveObject
{
    private const string NUGET_DOWNLOAD_BASE = "https://api.nuget.org/v3-flatcontainer";

    public IPlugin Plugin { get; init; }
    public bool HasDependencies => Plugin.Require.Length > 0;
    public bool IsNugetPackage => Plugin.PackageInfo is NugetPackage;
    [Reactive] public string NugetPackageUpdateState { get; set; } = MainLang.CheckingUpdate;
    [Reactive] public bool HasUpdate { get; set; } = false;
    [Reactive] public string? LatestVersion { get; set; }

    /// <summary>
    /// 更新NuGet包到最新版本
    /// </summary>
    /// <param name="targetVersion">目标版本，如果为null则更新到最新版本</param>
    /// <param name="sender">发送者控件，用于获取宿主ID</param>
    public async Task UpdateNugetPackage(Control? sender = null)
    {
        string targetVersion = null;
        if (Plugin.PackageInfo is not NugetPackage package)
        {
            Logger.Warning("尝试更新非NuGet包插件");
            return;
        }

        var hostId = sender != null ? GetHostId(sender) : "MainWindow";

        try
        {
            // 创建主任务
            var task = Tasking.CreateTask($"{MainLang.Update} {Plugin.Name}");
            var downloadSubTask = new TaskEntry($"{MainLang.Download} {package.Id}");
            var extractSubTask = new TaskEntry($"解压 {package.Id}");
            var replaceSubTask = new TaskEntry($"替换 {package.Id}");
            var cleanupSubTask = new TaskEntry($"清理临时文件");

            downloadSubTask.AddIn(task);
            extractSubTask.AddIn(task);
            replaceSubTask.AddIn(task);
            cleanupSubTask.AddIn(task);

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

            // 开始第一个子任务
            task.NextSubTask();

            // 获取目标版本
            string versionToDownload;
            if (string.IsNullOrEmpty(targetVersion))
            {
                versionToDownload = await GetLatestVersion(package.Id);
                if (string.IsNullOrEmpty(versionToDownload))
                {
                    throw new Exception("无法获取最新版本信息");
                }
            }
            else
            {
                versionToDownload = targetVersion;
            }

            // 构建下载URL和文件路径
            var downloadUrl =
                $"{NUGET_DOWNLOAD_BASE}/{package.Id.ToLowerInvariant()}/{versionToDownload}/{package.Id.ToLowerInvariant()}.{versionToDownload}.nupkg";
            var tempFileName = $"{package.Id}_temp_{versionToDownload}.nupkg"; // 临时文件保留版本号以避免冲突
            var tempFilePath = Path.Combine(ConfigPath.TempFolderPath, tempFileName);
            var originalFilePath = FindOriginalNupkgFile(package.Id);

            if (string.IsNullOrEmpty(originalFilePath))
            {
                task.FinishWithError();
                throw new Exception($"找不到原始的 {package.Id} 包文件");
            }

            // 配置下载器
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

                    downloadSubTask.ProgressValue = Math.Round(e.ProgressPercentage, 2);
                    downloadSubTask.ProgressIsIndeterminate = false;
                    downloadSubTask.TopRightInfoText = $"{speed}/s";
                    downloadSubTask.BottomLeftInfoText = $"{bytesReceived}/{totalBytes}";
                });
            };

            // 设置下载完成回调
            downloader.DownloadFileCompleted += async (_, args) =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (args.Cancelled)
                    {
                        task.CancelFinish();
                        return;
                    }

                    if (args.Error != null)
                    {
                        Logger.Error($"下载 {package.Id} 失败: {args.Error.Message}");
                        task.FinishWithError();
                        Notice($"{MainLang.DownloadFail}: {args.Error.Message}");
                        return;
                    }

                    downloadSubTask.FinishWithSuccess();
                    task.NextSubTask(); // 开始解压任务

                    try
                    {
                        await ProcessPackageUpdate(tempFilePath, originalFilePath, extractSubTask, replaceSubTask,
                            cleanupSubTask, task, package);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"处理包更新失败: {ex.Message}");
                        task.FinishWithError();
                        Notice($"更新失败: {ex.Message}");
                    }
                });
            };

            // 开始下载
            await downloader.DownloadFileTaskAsync(downloadUrl, tempFilePath, cts.Token);
        }
        catch (Exception ex)
        {
            Logger.Error($"更新 {package.Id} 时发生错误: {ex.Message}");
            Notice($"更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理包更新的后续步骤：解压、替换、清理
    /// </summary>
    private async Task ProcessPackageUpdate(string tempFilePath, string originalFilePath,
        TaskEntry extractSubTask, TaskEntry replaceSubTask, TaskEntry cleanupSubTask, TaskEntry mainTask, NugetPackage package)
    {
        try
        {
            // 解压步骤
            extractSubTask.TaskState = TaskState.Running;
            extractSubTask.ProgressIsIndeterminate = true;

            var tempExtractPath = Path.Combine(ConfigPath.TempFolderPath, $"extract_{Guid.NewGuid()}");
            Setter.TryCreateFolder(tempExtractPath);

            await Task.Run(() =>
            {
                using var archive = ZipFile.OpenRead(tempFilePath);
                archive.ExtractToDirectory(tempExtractPath, true);
            });

            extractSubTask.FinishWithSuccess();
            mainTask.NextSubTask(); // 开始替换任务

            // 替换步骤
            replaceSubTask.TaskState = TaskState.Running;
            replaceSubTask.ProgressIsIndeterminate = true;

            await Task.Run(() =>
            {
                // 备份原文件
                var backupPath = originalFilePath + ".backup";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                File.Move(originalFilePath, backupPath);

                // 确保最终文件名不包含版本号，使用新的.aupkg扩展名
                var finalFilePath = Path.Combine(ConfigPath.PluginFolderPath, $"{package.Id}.aupkg");

                // 复制新文件到最终位置（去除版本号）
                File.Copy(tempFilePath, finalFilePath, true);

                Logger.Info($"成功替换 {Path.GetFileName(finalFilePath)}");
            });

            replaceSubTask.FinishWithSuccess();
            mainTask.NextSubTask(); // 开始清理任务

            // 清理步骤
            cleanupSubTask.TaskState = TaskState.Running;
            cleanupSubTask.ProgressIsIndeterminate = true;

            await Task.Run(() =>
            {
                // 清理临时文件
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }

                Logger.Info("清理临时文件完成");
            });

            cleanupSubTask.FinishWithSuccess();
            mainTask.FinishWithSuccess();

            // 更新状态并通知用户
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                HasUpdate = false;
                NugetPackageUpdateState = MainLang.CurrentlyTheLatestVersion;
                LatestVersion = null;

                // 显示重启提示
                ShowRestartPrompt();
            });
        }
        catch (Exception ex)
        {
            Logger.Error($"处理包更新失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取包的最新版本
    /// </summary>
    private async System.Threading.Tasks.Task<string> GetLatestVersion(string packageId)
    {
        try
        {
            var versionsUrl = $"{NUGET_DOWNLOAD_BASE}/{packageId.ToLowerInvariant()}/index.json";
            var response = await versionsUrl
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithHeader("User-Agent", "Aurelio-App")
                .GetStringAsync();

            var versionsJson = JObject.Parse(response);
            var versions = versionsJson["versions"]?.ToObject<string[]>();

            if (versions == null || versions.Length == 0)
            {
                return string.Empty;
            }

            // 获取最新版本
            Version latestVersion = null;
            foreach (var versionString in versions)
            {
                if (Version.TryParse(versionString, out var version))
                {
                    if (latestVersion == null || version > latestVersion)
                    {
                        latestVersion = version;
                    }
                }
            }

            return latestVersion?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error($"获取 {packageId} 最新版本失败: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 查找原始的插件包文件路径（支持.aupkg和.nupkg格式，自动重命名.nupkg为.aupkg）
    /// </summary>
    private string FindOriginalNupkgFile(string packageId)
    {
        try
        {
            var pluginFolder = ConfigPath.PluginFolderPath;
            if (!Directory.Exists(pluginFolder))
            {
                return string.Empty;
            }

            // 首先查找.aupkg文件
            var aupkgFiles = Directory.GetFiles(pluginFolder, "*.aupkg", SearchOption.TopDirectoryOnly);

            // 然后查找.nupkg文件并自动重命名
            var nupkgFiles = Directory.GetFiles(pluginFolder, "*.nupkg", SearchOption.TopDirectoryOnly);
            var renamedFiles = new List<string>();

            foreach (var nupkgFile in nupkgFiles)
            {
                try
                {
                    var aupkgPath = Path.ChangeExtension(nupkgFile, ".aupkg");

                    // 检查是否已存在同名的.aupkg文件
                    if (!File.Exists(aupkgPath))
                    {
                        File.Move(nupkgFile, aupkgPath);
                        renamedFiles.Add(aupkgPath);
                        Logger.Info($"Auto-renamed {Path.GetFileName(nupkgFile)} to {Path.GetFileName(aupkgPath)}");
                    }
                    else
                    {
                        // 如果.aupkg已存在，删除.nupkg文件避免重复
                        File.Delete(nupkgFile);
                        Logger.Info($"Deleted duplicate {Path.GetFileName(nupkgFile)} as {Path.GetFileName(aupkgPath)} already exists");
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning($"Failed to rename {Path.GetFileName(nupkgFile)} to .aupkg: {e.Message}");
                    // 如果重命名失败，保留原文件
                    renamedFiles.Add(nupkgFile);
                }
            }

            var allFiles = new List<string>();
            allFiles.AddRange(aupkgFiles);
            allFiles.AddRange(renamedFiles);

            foreach (var file in allFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                // 首先尝试精确匹配（无版本号的文件名）
                if (fileName.Equals(packageId, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }

                // 然后尝试匹配带版本号的文件名（向后兼容）
                // 检查文件名是否以包ID开头，后面跟着版本号
                if (fileName.StartsWith(packageId + ".", StringComparison.OrdinalIgnoreCase))
                {
                    // 验证后面的部分是否为版本号格式
                    var versionPart = fileName.Substring(packageId.Length + 1);
                    if (IsValidVersionFormat(versionPart))
                    {
                        return file;
                    }
                }
            }

            Logger.Warning($"未找到 {packageId} 的原始插件包文件(.aupkg或.nupkg)");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error($"查找 {packageId} 原始文件时发生错误: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 验证字符串是否为有效的版本号格式
    /// </summary>
    private bool IsValidVersionFormat(string versionString)
    {
        if (string.IsNullOrEmpty(versionString))
            return false;

        // 尝试解析为版本号
        return Version.TryParse(versionString, out _) ||
               Regex.IsMatch(versionString, @"^\d+(\.\d+)*(-[a-zA-Z0-9\-\.]+)?$");
    }

    /// <summary>
    /// 显示重启提示对话框
    /// </summary>
    private async void ShowRestartPrompt()
    {
        try
        {
            var result = await ShowDialogAsync(
                title: MainLang.NeedRestartApp,
                msg: $"{Plugin.Name} {MainLang.UpdateCompleteRestartPrompt}",
                b_primary: MainLang.RestartNow,
                b_secondary: MainLang.RestartLater,
                b_cancel: MainLang.Cancel
            );

            switch (result)
            {
                case ContentDialogResult.Primary:
                    // 立即重启
                    Logger.Info($"用户选择立即重启应用以应用 {Plugin.Name} 的更新");
                    AppMethod.RestartApp();
                    break;

                case ContentDialogResult.Secondary:
                    // 稍后重启，显示通知
                    Notice($"{Plugin.Name} 更新完成，请稍后重启 Aurelio 以应用更新", NotificationType.Success,
                        TimeSpan.FromSeconds(5));
                    Logger.Info($"用户选择稍后重启应用以应用 {Plugin.Name} 的更新");
                    break;

                default:
                    // 取消，只显示成功通知
                    Notice($"{Plugin.Name} 更新完成", NotificationType.Success);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"显示重启提示时发生错误: {ex.Message}");
            // 如果对话框失败，至少显示通知
            Notice($"{Plugin.Name} 更新完成，请重启 Aurelio 以应用更新", NotificationType.Success);
        }
    }

    /// <summary>
    /// 删除插件
    /// </summary>
    /// <param name="sender">发送者控件，用于获取宿主ID</param>
    public async Task DeletePlugin(Control? sender = null)
    {
        try
        {
            // 显示确认对话框
            var title = Data.DesktopType == DesktopType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;

            var result = await ShowDialogAsync(
                title: title,
                msg: $"确定要删除插件 {Plugin.Name} 吗？\n\n• {Plugin.Name} v{Plugin.Version}\n• {Plugin.Id}",
                b_primary: MainLang.Ok,
                b_cancel: MainLang.Cancel,
                sender: sender
            );

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            // 查找插件文件
            var pluginFilePath = FindOriginalNupkgFile(Plugin.Id);
            if (string.IsNullOrEmpty(pluginFilePath) || !File.Exists(pluginFilePath))
            {
                Notice($"未找到插件文件: {Plugin.Id}", NotificationType.Error);
                Logger.Error($"未找到插件文件: {Plugin.Id}");
                return;
            }

            // 删除文件
            if (Data.DesktopType == DesktopType.Windows)
            {
                // Windows系统移动到回收站
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(pluginFilePath,
                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            }
            else
            {
                // 其他系统直接删除
                File.Delete(pluginFilePath);
            }

            Logger.Info($"成功删除插件: {Plugin.Name} ({Plugin.Id})");

            // 显示删除完成的重启提示
            ShowDeleteRestartPrompt();
        }
        catch (Exception ex)
        {
            Logger.Error($"删除插件 {Plugin.Name} 时发生错误: {ex.Message}");
            Notice($"删除插件失败: {ex.Message}", NotificationType.Error);
        }
    }

    /// <summary>
    /// 显示删除完成后的重启提示对话框
    /// </summary>
    private async void ShowDeleteRestartPrompt()
    {
        try
        {
            var result = await ShowDialogAsync(
                title: MainLang.NeedRestartApp,
                msg: $"{Plugin.Name} 删除完成！为了使更改生效，需要重启 Aurelio。是否现在重启？",
                b_primary: MainLang.RestartNow,
                b_secondary: MainLang.RestartLater,
                b_cancel: MainLang.Cancel
            );

            switch (result)
            {
                case ContentDialogResult.Primary:
                    // 立即重启
                    Logger.Info($"用户选择立即重启应用以应用 {Plugin.Name} 的删除");
                    AppMethod.RestartApp();
                    break;

                case ContentDialogResult.Secondary:
                    // 稍后重启，显示通知
                    Notice($"{Plugin.Name} 删除完成，请稍后重启 Aurelio 以应用更改", NotificationType.Success,
                        TimeSpan.FromSeconds(5));
                    Logger.Info($"用户选择稍后重启应用以应用 {Plugin.Name} 的删除");
                    break;

                default:
                    // 取消，只显示成功通知
                    Notice($"{Plugin.Name} 删除完成", NotificationType.Success);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"显示重启提示时发生错误: {ex.Message}");
            // 如果对话框失败，至少显示通知
            Notice($"{Plugin.Name} 删除完成，请重启 Aurelio 以应用更改", NotificationType.Success);
        }
    }
}