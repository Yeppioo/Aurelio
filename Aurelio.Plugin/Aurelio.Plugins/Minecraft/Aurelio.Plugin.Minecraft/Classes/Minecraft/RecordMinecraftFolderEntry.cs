namespace Aurelio.Plugin.Minecraft.Classes.Minecraft;

public class RecordMinecraftFolderEntry
{
    public string Path { get; set; }
    public string Name { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not RecordMinecraftFolderEntry entry) return false;
        return entry.Path == Path;
    }
}