namespace Aurelio.Public.Classes.Minecraft;

public class MinecraftFolderEntry
{
    public string Path { get; set; }
    public string Name { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not MinecraftFolderEntry entry) return false;
        return entry.Path == Path;
    }
}