using Aurelio.Public.Classes.Minecraft;

namespace Aurelio.Public.Module.Services.Minecraft.Launcher;

public class MinecraftClientLauncher
{
    public static void Launch(RecordMinecraftEntry entry)
    {
        var setting = Calculator.CalcMinecraftInstanceSetting(entry);
        
        
    }
}