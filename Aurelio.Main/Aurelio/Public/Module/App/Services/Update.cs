using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Http;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Downloader;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurelio.Public.Module.App.Services;

public class Update
{
    private const string GITHUB_API = "https://api.github.com/repos/Yeppioo/Aurelio/releases?per_page=1";
    private const string DOWNLOAD_BASE_URL = "https://github.com/Yeppioo/Aurelio/releases/download/auto-publish/";

    public static async Task<UpdateInfo> CheckUpdate()
    {
        var json = await GITHUB_API
            .WithHeader("User-Agent", "Aurelio-App")
            .GetStringAsync();

        var latest = JArray.Parse(json)[0];

        var info = new UpdateInfo
        {
            Body = latest["body"].ToString(),
            ReleaseTime = DateTime.Parse(latest["published_at"].ToString()),
            NewVersion = latest["name"].ToString(),
            IsNeedUpdate = latest["name"].ToString() != Data.Instance.Version
        };

        return info;
    }

    public static async Task UpdateApp(Control sender)
    {
        if (Data.DesktopType != DesktopType.Windows) return;
        if (Environment.OSVersion.Version.Major < 10) return;
        var file = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "Aurelio.win.x64.installer.exe",
            Architecture.X86 => "Aurelio.win.x86.installer.exe",
            Architecture.Arm64 => "Aurelio.win.arm64.installer.exe",
            _ => null
        };
        if (file.IsNullOrWhiteSpace()) return;

        var downloadOpt = new DownloadConfiguration()
        {
            ChunkCount = 5,
            ParallelDownload = true
        };

        var downloader = new DownloadService(downloadOpt);
        var cts = new CancellationTokenSource();

        var task = Tasking.CreateTask($"{MainLang.Update} App");
        new TaskEntry($"{MainLang.Download}: {file}") { TaskState = TaskState.Running }.AddIn(task);
        new TaskEntry($"{MainLang.BeginInstall}: {file}").AddIn(task);
        task.ProgressIsIndeterminate = false;
        task.ButtonAction = () =>
        {
            task.CancelWaitFinish();
            cts.Cancel();
            downloader.CancelAsync();
        };
        task.IsButtonEnable = true;
        task.ButtonText = MainLang.Cancel;
        task.TaskState = TaskState.Running;

        _ = OpenTaskDrawer(GetHostId(sender));

        downloader.DownloadProgressChanged += (o, e) =>
        {
            var estimateTime =
                (int)Math.Ceiling((e.TotalBytesToReceive - e.ReceivedBytesSize) / e.AverageBytesPerSecondSpeed);
            var timeLeftUnit = "s";

            if (estimateTime >= 60)
            {
                timeLeftUnit = "min";
                estimateTime /= 60;
            }
            else if (estimateTime < 0)
            {
                estimateTime = 0;
            }

            var bytesReceived = e.ReceivedBytesSize.CalcMemoryMensurableUnit();
            var totalBytesToReceive = e.TotalBytesToReceive.CalcMemoryMensurableUnit();
            task.ProgressValue = Math.Round(e.ProgressPercentage, 2);
            task.TopRightInfoText = $"{e.BytesPerSecondSpeed.CalcMemoryMensurableUnit()}/s";
            task.BottomLeftInfoText = $"[{bytesReceived}/{totalBytesToReceive}] {estimateTime} {timeLeftUnit} left";
        };

        downloader.DownloadFileCompleted += (_, args) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (args.Cancelled)
                {
                    task.CancelFinish();
                }
                else
                {
                    if (args.Error != null)
                    {
                        Logger.Error(args.Error);
                        task.FinishWithError();
                        Notice($"{MainLang.DownloadFail}: {args.Error.Message}");
                    }
                    else
                    {
                        task.FinishWithSuccess();
                        Notice($"{MainLang.DownloadFinish}: {file}", NotificationType.Success);

                        var startInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            WorkingDirectory = Environment.CurrentDirectory,
                            FileName = Path.Combine(ConfigPath.TempFolderPath, file!)
                        };
                        Process.Start(startInfo);
                        Environment.Exit(0);
                    }
                }
            });
        };

        await downloader.DownloadFileTaskAsync(
            Data.SettingEntry.EnableSpeedUpGithubApi
                ? Data.SettingEntry.GithubSpeedUpApiUrl.Replace("%url%", $"{DOWNLOAD_BASE_URL}{file}")
                : $"{DOWNLOAD_BASE_URL}{file}", Path.Combine
                (ConfigPath.TempFolderPath, file!), cts.Token);
    }

    public static async Task Download(string file, string path, Control sender)
    {
        var downloadOpt = new DownloadConfiguration()
        {
            ChunkCount = 5,
            ParallelDownload = true,
            // RequestConfiguration =
            // {
            //     Accept = "*/*",
            //     UserAgent =
            //         "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763",
            //     KeepAlive = true,
            //     Headers =
            //     [
            //         $"Host: {(Data.SettingEntry.EnableSpeedUpGithubApi ? new Uri(Data.SettingEntry.GithubSpeedUpApiUrl).Host : "github.com")}"
            //     ]
            // }
        };

        var downloader = new DownloadService(downloadOpt);

        var cts = new CancellationTokenSource();

        var task = Tasking.CreateTask($"{MainLang.Download}: {file}");
        new TaskEntry($"{MainLang.Download}: {file}") { TaskState = TaskState.Running }.AddIn(task);
        task.ProgressIsIndeterminate = false;
        task.ButtonAction = () =>
        {
            task.CancelWaitFinish();
            cts.Cancel();
            downloader.CancelAsync();
        };
        task.IsButtonEnable = true;
        task.ButtonText = MainLang.Cancel;
        task.TaskState = TaskState.Running;

        _ = OpenTaskDrawer(GetHostId(sender));

        downloader.DownloadProgressChanged += (o, e) =>
        {
            var estimateTime =
                (int)Math.Ceiling((e.TotalBytesToReceive - e.ReceivedBytesSize) / e.AverageBytesPerSecondSpeed);
            var timeLeftUnit = "s";

            if (estimateTime >= 60)
            {
                timeLeftUnit = "min";
                estimateTime /= 60;
            }
            else if (estimateTime < 0)
            {
                estimateTime = 0;
            }

            var bytesReceived = e.ReceivedBytesSize.CalcMemoryMensurableUnit();
            var totalBytesToReceive = e.TotalBytesToReceive.CalcMemoryMensurableUnit();
            task.ProgressValue = Math.Round(e.ProgressPercentage, 2);
            task.TopRightInfoText = $"{e.BytesPerSecondSpeed.CalcMemoryMensurableUnit()}/s";
            task.BottomLeftInfoText = $"[{bytesReceived}/{totalBytesToReceive}] {estimateTime} {timeLeftUnit} left";
        };

        downloader.DownloadFileCompleted += (_, args) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (args.Cancelled)
                {
                    task.CancelFinish();
                }
                else
                {
                    if (args.Error != null)
                    {
                        Logger.Error(args.Error);
                        task.FinishWithError();
                        Notice($"{MainLang.DownloadFail}: {args.Error.Message}");
                    }
                    else
                    {
                        task.FinishWithSuccess();
                        Notice($"{MainLang.DownloadFinish}: {file}", NotificationType.Success);
                    }
                }
            });
        };

        await downloader.DownloadFileTaskAsync(
            Data.SettingEntry.EnableSpeedUpGithubApi
                ? Data.SettingEntry.GithubSpeedUpApiUrl.Replace("%url%", $"{DOWNLOAD_BASE_URL}{file}")
                : $"{DOWNLOAD_BASE_URL}{file}", path, cts.Token);
    }
}