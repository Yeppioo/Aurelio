namespace Aurelio.Plugin.Minecraft.Classes.Minecraft;

public class MinecraftLaunchSettingEntry
{
    public double MemoryLimit { get; set; }
    public bool EnableIndependentMinecraft { get; set; }
    public string AutoJoinServerAddress { get; set; }
    public RecordJavaRuntime JavaRuntime { get; set; }
}