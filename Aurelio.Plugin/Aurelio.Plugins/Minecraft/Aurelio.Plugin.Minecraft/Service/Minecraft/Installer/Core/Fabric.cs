using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.IO;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;

namespace Aurelio.Plugin.Minecraft.Service.Minecraft.Installer.Core;

public class Fabric
{
    public static async Task<bool> Main(FabricInstallEntry entry, string path, string customId,
        TaskEntry mainTask, TaskEntry task1, TaskEntry task2, CancellationToken cancellationToken)
    {
        try
        {
            var installer = FabricInstaller.Create(path, entry, customId);
            task1.TaskState = TaskState.Running;
            installer.ProgressChanged += (_, arg) =>
            {
                if (arg.StepName == InstallStep.InstallPrimaryModLoader)
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

            await installer.InstallAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return false;
        }
    }
}