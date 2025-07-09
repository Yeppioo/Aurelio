using Aurelio.Public.Classes.Enum.Minecraft;
using Avalonia.Media.Imaging;

namespace Aurelio.Public.Classes.Minecraft;

public class MinecraftInstanceSettingEntry
{
    public DateTime LastPlayed { get; set; } = DateTime.MinValue;
    public MinecraftInstanceIconType IconType { get; set; } = MinecraftInstanceIconType.Auto;
    public string IconData { get; set; }
    
}