using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Installer;

namespace Aurelio.Plugin.Minecraft.Service.Minecraft.Installer.Core.VersionFetcher;

public static class Vanilla
{
    public static async Task<IEnumerable<VersionManifestEntry>> EnumerableMinecraftAsync()
    {
        return await VanillaInstaller.EnumerableMinecraftAsync();
    }
}