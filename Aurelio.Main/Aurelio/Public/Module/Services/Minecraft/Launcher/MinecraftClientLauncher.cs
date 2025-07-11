using Aurelio.Public.Classes.Minecraft;

namespace Aurelio.Public.Module.Services.Minecraft.Launcher;

public class MinecraftClientLauncher
{
    public static async Task Launch(RecordMinecraftEntry entry)
    {
        MinecraftLaunchSettingEntry setting;

        
            setting = Calculator.CalcMinecraftInstanceSetting(entry);
    
        
        
    }
}