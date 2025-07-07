using Avalonia.Media.Imaging;

namespace Aurelio.Public.Classes.Minecraft;

public class MinecraftInstanceSettingEntry
{
    public DateTime LastPlayed { get; set; } = DateTime.MinValue;
    public Bitmap Icon { get; set; }
}