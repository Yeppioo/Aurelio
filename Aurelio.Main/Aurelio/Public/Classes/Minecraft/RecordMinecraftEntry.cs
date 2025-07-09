using System.Linq;
using Aurelio.Public.Classes.Enum.Minecraft;
using Aurelio.Public.Module.Value;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Public.Classes.Minecraft;

public class RecordMinecraftEntry
{
    public string Id { get; init; }
    public MinecraftVersionType Type => MlEntry.Version.Type;
    public MinecraftEntry MlEntry { get; }
    public string ShortDescription => $"{Loader} {MlEntry.Version.VersionId}";
    public string Loader { get; }
    public string[] Tags { get; } = [];
    public string[] Loaders { get; }
    public Bitmap Icon { get; }
    public MinecraftInstanceSettingEntry SettingEntry { get; } = new();
    public RecordMinecraftFolderEntry? ParentMinecraftFolder => Module.Value.Minecraft.Calculator.GetMinecraftFolderByEntry(MlEntry);
    public string InstancePath => Module.Value.Minecraft.Calculator.GetMinecraftSpecialFolder(MlEntry, MinecraftSpecialFolder.InstanceFolder);
    public RecordMinecraftEntry(MinecraftEntry mlEntry)
    {
        Id = mlEntry.Id;
        MlEntry = mlEntry;
        Loader = mlEntry.IsVanilla
            ? "Vanilla" : string.Join(", ", (mlEntry as ModifiedMinecraftEntry)?
                .ModLoaders.Select(x => $"{x.Type}")!);
        Loaders = mlEntry.IsVanilla
            ? ["Vanilla"]
            : (mlEntry as ModifiedMinecraftEntry)?
            .ModLoaders.Select(x => $"{x.Type}").ToArray();
        Icon = Module.Value.Minecraft.Calculator.GetMinecraftInstanceIcon(this);
    }
}