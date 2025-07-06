using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Public.Classes.Minecraft;

public class RecordMinecraftEntry
{
    public string Id { get; init; }
    public MinecraftEntry MlEntry { get; }
    public string ShortDescription => $"{Loaders} {MlEntry.Version.VersionId}";
    public string Loaders { get; }
    public Bitmap Icon  { get; }
 
    public RecordMinecraftEntry(MinecraftEntry mlEntry)
    {
        Id = mlEntry.Id;
        MlEntry = mlEntry;
        Loaders = mlEntry.IsVanilla
            ? "Vanilla" : string.Join(", ", (mlEntry as ModifiedMinecraftEntry)?
                .ModLoaders.Select(x => $"{x.Type}")!);
        Icon = GetMinecraftIcon(this);
    }
    
    public static Bitmap GetMinecraftIcon(RecordMinecraftEntry entry)
    {
        if (entry.MlEntry.IsVanilla)
        {
            return entry.MlEntry.Version.Type switch
            {
                MinecraftVersionType.Release => Module.IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.grass_block_side.png"),
                MinecraftVersionType.Snapshot => Module.IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.crafting_table_front.png"),
                _ => Module.IO.Local.Getter.LoadBitmapFromAppFile(
                    "Aurelio.Public.Assets.McIcons.grass_block_side.png")
            };
        }

        if (entry.MlEntry is not ModifiedMinecraftEntry e)
            return Module.IO.Local.Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.grass_block_side.png");
        if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Forge))
        {
            return Module.IO.Local.Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.furnace_front.png");
        }

        if (e.ModLoaders.Any(a => a.Type == ModLoaderType.NeoForge))
        {
            return Module.IO.Local.Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.NeoForgeIcon.png");
        }

        if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Fabric))
        {
            return Module.IO.Local.Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.FabricIcon.png");
        }

        if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Quilt))
        {
            return Module.IO.Local.Getter.LoadBitmapFromAppFile(
                "Aurelio.Public.Assets.McIcons.QuiltIcon.png");
        }

        return Module.IO.Local.Getter.LoadBitmapFromAppFile(
            e.ModLoaders.Any(a => a.Type == ModLoaderType.OptiFine)
                ? "Aurelio.Public.Assets.McIcons.OptiFineIcon.png"
                : "Aurelio.Public.Assets.McIcons.grass_block_side.png");
    }
}