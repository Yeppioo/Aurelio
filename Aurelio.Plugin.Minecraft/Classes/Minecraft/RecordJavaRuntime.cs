using MinecraftLaunch.Base.Models.Game;

namespace Aurelio.Plugin.Minecraft.Classes.Minecraft;

public class RecordJavaRuntime
{
    public bool Is64bit { get; init; }
    public string JavaPath { get; set; }
    public string JavaType { get; init; }
    public string JavaVersion { get; init; }
    public string JavaFolder { get; init; }
    public int MajorVersion { get; init; }

    public override bool Equals(object? obj)
    {
        if (obj is not RecordJavaRuntime other) return false;
        if (other.JavaVersion == "auto" && JavaVersion == "auto") return true;
        if (other.JavaVersion == "global" && JavaVersion == "global") return true;
        return Is64bit == other.Is64bit && JavaPath == other.JavaPath && JavaType == other.JavaType;
    }

    public static RecordJavaRuntime MlToAurelio(JavaEntry entry)
    {
        return new RecordJavaRuntime
        {
            Is64bit = entry.Is64bit,
            JavaVersion = entry.JavaVersion,
            JavaPath = entry.JavaPath,
            JavaType = entry.JavaType,
            JavaFolder = entry.JavaFolder,
            MajorVersion = entry.MajorVersion
        };
    }

    public static JavaEntry AurelioToMl(RecordJavaRuntime entry)
    {
        return new JavaEntry
        {
            Is64bit = entry.Is64bit,
            JavaVersion = entry.JavaVersion,
            JavaPath = entry.JavaPath,
            JavaType = entry.JavaType,
            MajorVersion = entry.MajorVersion
        };
    }
}