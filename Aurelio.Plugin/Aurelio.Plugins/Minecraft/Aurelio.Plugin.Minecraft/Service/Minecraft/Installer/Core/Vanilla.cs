using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.IO;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;

namespace Aurelio.Plugin.Minecraft.Service.Minecraft.Installer.Core;

public class Vanilla
{
    public static async Task<(bool success, MinecraftEntry? entry)> Main(VersionManifestEntry version, string path,
        string customId, TaskEntry mainTask, TaskEntry task1, TaskEntry task2, CancellationToken cancellationToken)
    {
        try
        {
            var installer = VanillaInstaller.Create(path, version);
            task1.TaskState = TaskState.Running;
            installer.ProgressChanged += (_, arg) =>
            {
                if (arg.StepName == InstallStep.DownloadLibraries)
                {
                    task1.FinishWithSuccess();
                    task2.TaskState = TaskState.Running;
                }
                mainTask.ProgressIsIndeterminate = false;
                mainTask.BottomLeftInfoText =
                    $"{arg.StepName} - {arg.FinishedStepTaskCount}/{arg.TotalStepTaskCount}";
                if (arg.IsStepSupportSpeed)
                    mainTask.TopRightInfoText = $"{DefaultDownloader.FormatSize(arg.Speed, true)}";
                mainTask.ProgressValue = Math.Round(arg.Progress * 100, 2);
            };

            var minecraft = await installer.InstallAsync(cancellationToken);
            return (true, minecraft);
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return (false, null);
        }
    }
}