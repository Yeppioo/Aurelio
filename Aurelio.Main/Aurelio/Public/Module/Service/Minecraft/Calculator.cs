using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.IO.Local;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Extensions;

namespace Aurelio.Public.Module.Service.Minecraft;

public class Calculator
{
    public static MinecraftLaunchSettingEntry CalcMinecraftInstanceSetting(RecordMinecraftEntry entry)
    {
        RecordJavaRuntime javaRuntime = null;

        if (entry.SettingEntry.JavaRuntime.JavaVersion == "global")
        {
            if (Data.SettingEntry.PreferredJavaRuntime.JavaVersion == "auto")
                javaRuntime =
                    GetCurrentJava(Data.SettingEntry.JavaRuntimes.Select(RecordJavaRuntime.AurelioToMl).ToList(),
                        entry.MlEntry);
            else
                javaRuntime = Data.SettingEntry.PreferredJavaRuntime;
        }
        else if (entry.SettingEntry.JavaRuntime.JavaVersion == "auto")
        {
            javaRuntime = GetCurrentJava(
                Data.SettingEntry.JavaRuntimes.Select(RecordJavaRuntime.AurelioToMl).ToList(),
                entry.MlEntry);
        }
        else
        {
            javaRuntime = entry.SettingEntry.JavaRuntime;
        }

        var memoryLimit = entry.SettingEntry.MemoryLimit < 0
            ? Data.SettingEntry.MemoryLimit
            : entry.SettingEntry.MemoryLimit;

        var enableIndependentMinecraft = entry.SettingEntry.EnableIndependentMinecraft == 0
            ? Data.SettingEntry.EnableIndependentMinecraft
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
            return Value.Converter.Base64ToBitmap(entry.SettingEntry.IconData);

        if (type == MinecraftInstanceIconType.CraftingTable)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.crafting_table_front.png");

        if (type == MinecraftInstanceIconType.Furnace)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.furnace_front.png");

        if (type == MinecraftInstanceIconType.DirtPath)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.dirt_path_side.png");

        if (type == MinecraftInstanceIconType.GrassBlock)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.grass_block_side.png");

        if (type == MinecraftInstanceIconType.GlassBlock)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.OptiFineIcon.png");

        if (type == MinecraftInstanceIconType.Quilt)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.QuiltIcon.png");

        if (type == MinecraftInstanceIconType.Forge)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.ForgeIcon.png");

        if (type == MinecraftInstanceIconType.Fabric)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.FabricIcon.png");

        if (type == MinecraftInstanceIconType.NoeForge)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.NeoForgeIcon.png");

        if (type == MinecraftInstanceIconType.OptiFine)
            return Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.OptiFineIcon.png");

        return Getter.LoadBitmapFromAppFile(
            "Aurelio.Public.Assets.McIcons.grass_block_side.png");

        static Bitmap GetEmbeddedIcon(RecordMinecraftEntry entry)
        {
            if (entry.MlEntry.IsVanilla)
                return entry.MlEntry.Version.Type switch
                {
                    MinecraftVersionType.Release => Getter.LoadBitmapFromAppFile(
                        "Aurelio.Public.Assets.McIcons.grass_block_side.png"),
                    MinecraftVersionType.Snapshot => Getter.LoadBitmapFromAppFile(
                        "Aurelio.Public.Assets.McIcons.crafting_table_front.png"),
                    _ => Getter.LoadBitmapFromAppFile(
                        "Aurelio.Public.Assets.McIcons.grass_block_side.png")
                };

            if (entry.MlEntry is not ModifiedMinecraftEntry e)
                return Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.grass_block_side.png");
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Forge))
                return Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.furnace_front.png");

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.NeoForge))
                return Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.NeoForgeIcon.png");

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Fabric))
                return Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.FabricIcon.png");

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Quilt))
                return Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.QuiltIcon.png");

            return Getter.LoadBitmapFromAppFile(
                e.ModLoaders.Any(a => a.Type == ModLoaderType.OptiFine)
                    ? "Aurelio.Public.Assets.McIcons.OptiFineIcon.png"
                    : "Aurelio.Public.Assets.McIcons.grass_block_side.png");
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
        return Data.SettingEntry.MinecraftFolderEntries.FirstOrDefault(x => entry.MinecraftFolderPath == x.Path);
    }
}