using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Aurelio.Plugin.Minecraft.Classes.Enum.Minecraft;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Public.Module.IO.Local;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Extensions;

namespace Aurelio.Plugin.Minecraft.Service.Minecraft;

public class Calculator
{
    /// <summary>
    /// 从插件程序集中加载位图资源
    /// </summary>
    /// <param name="uri">资源URI</param>
    /// <param name="width">宽度</param>
    /// <returns>位图</returns>
    private static Bitmap LoadBitmapFromPluginAssembly(string uri, int width = 48)
    {
        // 获取当前插件程序集
        var assembly = Assembly.GetExecutingAssembly();

        // 从程序集中获取嵌入的资源流
        var stream = assembly.GetManifestResourceStream(uri);
        if (stream == null)
        {
            // 列出所有可用的资源以便调试
            var availableResources = assembly.GetManifestResourceNames();
            var resourceList = string.Join(", ", availableResources);
            throw new FileNotFoundException($"Resource '{uri}' not found in assembly '{assembly.FullName}'. Available resources: {resourceList}");
        }

        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        stream.Dispose();

        return Bitmap.DecodeToWidth(memoryStream, width);
    }
    public static MinecraftLaunchSettingEntry CalcMinecraftInstanceSetting(RecordMinecraftEntry entry)
    {
        RecordJavaRuntime javaRuntime = null;

        if (entry.SettingEntry.JavaRuntime.JavaVersion == "global")
        {
            if (MinecraftPluginData.MinecraftPluginSettingEntry.PreferredJavaRuntime.JavaVersion == "auto")
                javaRuntime =
                    GetCurrentJava(MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Select(RecordJavaRuntime.AurelioToMl).ToList(),
                        entry.MlEntry);
            else
                javaRuntime = MinecraftPluginData.MinecraftPluginSettingEntry.PreferredJavaRuntime;
        }
        else if (entry.SettingEntry.JavaRuntime.JavaVersion == "auto")
        {
            javaRuntime = GetCurrentJava(
                MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Select(RecordJavaRuntime.AurelioToMl).ToList(),
                entry.MlEntry);
        }
        else
        {
            javaRuntime = entry.SettingEntry.JavaRuntime;
        }

        var memoryLimit = entry.SettingEntry.MemoryLimit < 0
            ? MinecraftPluginData.MinecraftPluginSettingEntry.MemoryLimit
            : entry.SettingEntry.MemoryLimit;

        var enableIndependentMinecraft = entry.SettingEntry.EnableIndependentMinecraft == 0
            ? MinecraftPluginData.MinecraftPluginSettingEntry.EnableIndependentMinecraft
            : entry.SettingEntry.EnableIndependentMinecraft == 1;

        var AutoJoinServerAddress = entry.SettingEntry.AutoJoinServerAddress;

        return new MinecraftLaunchSettingEntry
        {
            JavaRuntime = javaRuntime,
            MemoryLimit = memoryLimit,
            EnableIndependentMinecraft = enableIndependentMinecraft,
            AutoJoinServerAddress = AutoJoinServerAddress
        };
    }

    public static Guid NameToMcOfflineUUID(string name)
    {
        var inputBytes = Encoding.UTF8.GetBytes("OfflinePlayer:" + name);
        var hash = MD5.HashData(inputBytes);

        hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash);
    }
    
    public static RecordJavaRuntime GetCurrentJava(List<JavaEntry> javaEntries, MinecraftEntry game)
    {
        return RecordJavaRuntime.MlToAurelio(game.GetAppropriateJava(javaEntries));
    }

    public static Bitmap GetMinecraftInstanceIcon(RecordMinecraftEntry entry)
    {
        var type = entry.SettingEntry.IconType;
        if (type == MinecraftInstanceIconType.Auto)
        {
            if (File.Exists(Path.Combine(GetMinecraftSpecialFolder
                    (entry.MlEntry, MinecraftSpecialFolder.InstanceFolder), "icon.png")))
            {
                var s = File.OpenRead(Path.Combine(GetMinecraftSpecialFolder
                    (entry.MlEntry, MinecraftSpecialFolder.InstanceFolder), "icon.png"));
                return Bitmap.DecodeToWidth(s, 48);
            }

            if (!File.Exists(Path.Combine(GetMinecraftSpecialFolder
                    (entry.MlEntry, MinecraftSpecialFolder.InstanceFolder), "PCL", "Logo.png")))
                return GetEmbeddedIcon(entry);
            {
                var s = File.OpenRead(Path.Combine(GetMinecraftSpecialFolder
                    (entry.MlEntry, MinecraftSpecialFolder.InstanceFolder), "PCL", "Logo.png"));
                return Bitmap.DecodeToWidth(s, 48);
            }
        }

        if (type == MinecraftInstanceIconType.Base64)
            return Public.Module.Value.Converter.Base64ToBitmap(entry.SettingEntry.IconData);

        if (type == MinecraftInstanceIconType.CraftingTable)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.crafting_table_front.png");

        if (type == MinecraftInstanceIconType.Furnace)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.furnace_front.png");

        if (type == MinecraftInstanceIconType.DirtPath)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.dirt_path_side.png");

        if (type == MinecraftInstanceIconType.GrassBlock)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.grass_block_side.png");

        if (type == MinecraftInstanceIconType.GlassBlock)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.OptiFineIcon.png");

        if (type == MinecraftInstanceIconType.Quilt)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.QuiltIcon.png");

        if (type == MinecraftInstanceIconType.Forge)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.ForgeIcon.png");

        if (type == MinecraftInstanceIconType.Fabric)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.FabricIcon.png");

        if (type == MinecraftInstanceIconType.NoeForge)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.NeoForgeIcon.png");

        if (type == MinecraftInstanceIconType.OptiFine)
            return LoadBitmapFromPluginAssembly(
                "Aurelio.Plugin.Minecraft.Assets.McIcons.OptiFineIcon.png");

        return LoadBitmapFromPluginAssembly(
            "Aurelio.Plugin.Minecraft.Assets.McIcons.grass_block_side.png");

        static Bitmap GetEmbeddedIcon(RecordMinecraftEntry entry)
        {
            if (entry.MlEntry.IsVanilla)
                return entry.MlEntry.Version.Type switch
                {
                    MinecraftVersionType.Release => LoadBitmapFromPluginAssembly(
                        "Aurelio.Plugin.Minecraft.Assets.McIcons.grass_block_side.png"),
                    MinecraftVersionType.Snapshot => LoadBitmapFromPluginAssembly(
                        "Aurelio.Plugin.Minecraft.Assets.McIcons.crafting_table_front.png"),
                    _ => LoadBitmapFromPluginAssembly(
                        "Aurelio.Plugin.Minecraft.Assets.McIcons.grass_block_side.png")
                };

            if (entry.MlEntry is not ModifiedMinecraftEntry e)
                return LoadBitmapFromPluginAssembly(
                    "Aurelio.Plugin.Minecraft.Assets.McIcons.grass_block_side.png");
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Forge))
                return LoadBitmapFromPluginAssembly(
                    "Aurelio.Plugin.Minecraft.Assets.McIcons.furnace_front.png");

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.NeoForge))
                return LoadBitmapFromPluginAssembly(
                    "Aurelio.Plugin.Minecraft.Assets.McIcons.NeoForgeIcon.png");

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Fabric))
                return LoadBitmapFromPluginAssembly(
                    "Aurelio.Plugin.Minecraft.Assets.McIcons.FabricIcon.png");

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Quilt))
                return LoadBitmapFromPluginAssembly(
                    "Aurelio.Plugin.Minecraft.Assets.McIcons.QuiltIcon.png");

            return LoadBitmapFromPluginAssembly(
                e.ModLoaders.Any(a => a.Type == ModLoaderType.OptiFine)
                    ? "Aurelio.Plugin.Minecraft.Assets.McIcons.OptiFineIcon.png"
                    : "Aurelio.Plugin.Minecraft.Assets.McIcons.grass_block_side.png");
        }
    }

    public static string GetMinecraftSpecialFolder(MinecraftEntry entry, MinecraftSpecialFolder folder,
        bool isForceEnableIndependencyCore = false)
    {
        // var setting = MinecraftSetting.GetGameSetting(entry);
        // MinecraftSetting.HandleGameSetting(setting);
        // var isEnableIndependencyCore = isForceEnableIndependencyCore || setting.IsEnableIndependencyCore;
        var basePath = true
            ? Path.Combine(entry.MinecraftFolderPath, "versions", entry.Id)
            : entry.MinecraftFolderPath;
        var path = folder switch
        {
            MinecraftSpecialFolder.InstanceFolder => basePath,
            MinecraftSpecialFolder.ModsFolder => Path.Combine(basePath, "mods"),
            MinecraftSpecialFolder.ResourcePacksFolder => Path.Combine(basePath, "resourcepacks"),
            MinecraftSpecialFolder.SavesFolder => Path.Combine(basePath, "saves"),
            MinecraftSpecialFolder.ScreenshotsFolder => Path.Combine(basePath, "screenshots"),
            MinecraftSpecialFolder.ShaderPacksFolder => Path.Combine(basePath, "shaderpacks"),
            _ => basePath
        };
        Setter.TryCreateFolder(path);
        return path;
    }

    public static RecordMinecraftFolderEntry? GetMinecraftFolderByEntry(MinecraftEntry entry)
    {
        return MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries.FirstOrDefault(x => entry.MinecraftFolderPath == x.Path);
    }
}