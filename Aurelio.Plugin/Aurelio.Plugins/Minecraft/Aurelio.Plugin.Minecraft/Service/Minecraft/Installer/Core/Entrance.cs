using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Base.Models.Network;

namespace Aurelio.Plugin.Minecraft.Service.Minecraft.Installer.Core;

public class Entrance
{
    public static async Task Main(VersionManifestEntry version,
        string path, string customId,
        OptifineInstallEntry? optifineInstallEntry = null,
        FabricInstallEntry? fabricInstallEntry = null,
        ForgeInstallEntry? forgeInstallEntry = null,
        QuiltInstallEntry? quiltInstallEntry = null)
    {
        var cts = new CancellationTokenSource();
        var task = Tasking.CreateTask($"{MainLang.Install}: {customId} ({version.Id})");
        var jPath = MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes
            .First(x => x.JavaFolder != null).JavaPath;

        task.ButtonText = MainLang.Cancel;
        task.ButtonAction = () =>
        {
            cts.Cancel();
            task.CancelWaitFinish();
            Directory.Delete(Path.Combine(path, "versions", customId), true);
        };
        task.TaskState = TaskState.Running;

        var vanillaTask = new TaskEntry($"Minecraft {version.Id}").AddIn(task);
        var vanillaCheckTask = new TaskEntry(MainLang.CheckVersionResource).AddIn(vanillaTask);
        var vanillaDownloadTask = new TaskEntry(MainLang.DownloadResource).AddIn(vanillaTask);

        var forgeTask = new TaskEntry($"Forge {forgeInstallEntry?.ForgeVersion}");
        var forgeDownloadTask = new TaskEntry(MainLang.DownloadResource).AddIn(forgeTask);
        var forgeInstallTask =
            new TaskEntry(forgeInstallEntry != null && forgeInstallEntry.IsNeoforge
                ? $"{MainLang.Install} NeoForge"
                : $"{MainLang.Install} Forge").AddIn(forgeTask);

        var fabricTask = new TaskEntry($"Fabric {fabricInstallEntry?.BuildVersion}");
        var fabricDownloadTask = new TaskEntry(MainLang.DownloadResource).AddIn(fabricTask);
        var fabricInstallTask = new TaskEntry($"{MainLang.Install} Fabric").AddIn(fabricTask);

        var optifineTask = new TaskEntry($"Optifine {optifineInstallEntry?.Type} {optifineInstallEntry?.Patch}");
        var optifineDownloadTask = new TaskEntry(MainLang.DownloadResource).AddIn(optifineTask);
        var optifineInstallTask = new TaskEntry($"{MainLang.Install} Optifine").AddIn(optifineTask);

        var quiltTask = new TaskEntry($"Quilt {quiltInstallEntry?.BuildVersion}");
        var quiltDownloadTask = new TaskEntry(MainLang.DownloadResource).AddIn(quiltTask);
        var quiltInstallTask = new TaskEntry($"{MainLang.Install} Quilt").AddIn(quiltTask);

        if (forgeInstallEntry != null)
        {
            task.SubTasks.Add(forgeTask);
        }

        if (fabricInstallEntry != null)
        {
            task.SubTasks.Add(fabricTask);
        }

        if (optifineInstallEntry != null)
        {
            task.SubTasks.Add(optifineTask);
        }

        if (quiltInstallEntry != null)
        {
            task.SubTasks.Add(quiltTask);
        }

        vanillaTask.TaskState = TaskState.Running;
        var vanillaSuccess = 
            await Vanilla.Main(version, path, customId, task, vanillaCheckTask, vanillaDownloadTask, cts.Token);
        if (cts.IsCancellationRequested)
        {
            task.CancelFinish();
            return;
        }
        if (!vanillaSuccess)
        {
            Notice($"{MainLang.InstallFail}: Minecraft {version.Id}", NotificationType.Error);
            task.FinishWithError();
            return;
        }
        vanillaTask.FinishWithSuccess();

        if (forgeInstallEntry != null)
        {
            forgeTask.TaskState = TaskState.Running;
            var forgeInstallSuccess = await Forge.Main(forgeInstallEntry, path , jPath, customId, task, forgeDownloadTask,
                forgeInstallTask, cts.Token);
            if (cts.IsCancellationRequested)
            {
                task.CancelFinish();
                return;
            }
            if (!forgeInstallSuccess)
            {
                Notice($"{MainLang.InstallFail}: {version.Id} {(forgeInstallEntry.IsNeoforge ? "NeoForge" : "Forge")}",
                    NotificationType.Error);
                task.FinishWithError();
                return;
            }
        }
        forgeTask.FinishWithSuccess();

        if (fabricInstallEntry != null)
        {
            fabricTask.TaskState = TaskState.Running;
            var fabricInstallSuccess = await Fabric.Main(fabricInstallEntry, path, customId, task,
                fabricDownloadTask,
                fabricInstallTask, cts.Token);
            if (cts.IsCancellationRequested)
            {
                task.CancelFinish();
                return;
            }
            if (!fabricInstallSuccess)
            {
                Notice($"{MainLang.InstallFail}: {version.Id} Fabric", NotificationType.Error);
                task.FinishWithError();
                return;
            }
        }
        fabricTask.FinishWithSuccess();

        if (optifineInstallEntry != null)
        {
            optifineTask.TaskState = TaskState.Running;
            var optifineInstallSuccess = await Optifine.Main(optifineInstallEntry, path , jPath, customId, task,
                optifineDownloadTask,
                optifineInstallTask, cts.Token);
            if (cts.IsCancellationRequested)
            {
                task.CancelFinish();
                return;
            }
            if (!optifineInstallSuccess)
            {
                Notice($"{MainLang.InstallFail}: {version.Id} Optifine", NotificationType.Error);
                task.FinishWithError();
                return;
            }
        }
        optifineTask.FinishWithSuccess();

        if (quiltInstallEntry != null)
        {
            quiltTask.TaskState = TaskState.Running;
            var quiltInstallSuccess = await Quilt.Main(quiltInstallEntry, path, customId, quiltTask, task,
                quiltInstallTask, cts.Token);
            if (cts.IsCancellationRequested)
            {
                task.CancelFinish();
                return;
            }
            if (!quiltInstallSuccess)
            {
                Notice($"{MainLang.InstallFail}: {version.Id} Quilt", NotificationType.Error);
                task.FinishWithError();
                return;
            }
        }
        quiltTask.FinishWithSuccess();

        if (cts.IsCancellationRequested)
        {
            task.CancelFinish();
            return;
        }
        Notice($"{MainLang.InstallFinish}: {version.Id}", NotificationType.Success);
        task.FinishWithSuccess();
    }
}