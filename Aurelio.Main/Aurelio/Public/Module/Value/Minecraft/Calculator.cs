using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Enum.Minecraft;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Public.Module.Value.Minecraft;

public class Calculator
{
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
        {
            return Converter.Base64ToBitmap(entry.SettingEntry.IconData);
        }

        return type == MinecraftInstanceIconType.Path
            ? Bitmap.DecodeToWidth(File.OpenRead(entry.SettingEntry.IconData), 48)
            : GetEmbeddedIcon(entry);


        static Bitmap GetEmbeddedIcon(RecordMinecraftEntry entry)
        {
            if (entry.MlEntry.IsVanilla)
            {
                return entry.MlEntry.Version.Type switch
                {
                    MinecraftVersionType.Release => IO.Local.Getter.LoadBitmapFromAppFile(
                        "Aurelio.Public.Assets.McIcons.grass_block_side.png"),
                    MinecraftVersionType.Snapshot => IO.Local.Getter.LoadBitmapFromAppFile(
                        "Aurelio.Public.Assets.McIcons.crafting_table_front.png"),
                    _ => IO.Local.Getter.LoadBitmapFromAppFile(
                        "Aurelio.Public.Assets.McIcons.grass_block_side.png")
                };
            }

            if (entry.MlEntry is not ModifiedMinecraftEntry e)
                return IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.grass_block_side.png");
            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Forge))
            {
                return IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.furnace_front.png");
            }

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.NeoForge))
            {
                return IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.NeoForgeIcon.png");
            }

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Fabric))
            {
                return IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.FabricIcon.png");
            }

            if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Quilt))
            {
                return IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.QuiltIcon.png");
            }

            return IO.Local.Getter.LoadBitmapFromAppFile(
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
            : entry.MinecraftFolderPath; //TODO
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
        IO.Local.Setter.TryCreateFolder(path);
        return path;
    }
    
    public static RecordMinecraftFolderEntry? GetMinecraftFolderByEntry(MinecraftEntry entry)
    {
        return Data.SettingEntry.MinecraftFolderEntries.FirstOrDefault(x => entry.MinecraftFolderPath == x.Path);
    }
}