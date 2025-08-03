using Aurelio.Public.Classes.Entries;
using MinecraftLaunch.Base.Models.Network;

namespace Aurelio.Plugin.Minecraft.Service.Minecraft.Installer.Core;

public class Optifine
{
    public static async Task<bool> Main(OptifineInstallEntry version, string path, string customId,
        TaskEntry mainTask, TaskEntry task1, TaskEntry task2, CancellationToken cancellationToken)
    {
        return true;  
    }
}