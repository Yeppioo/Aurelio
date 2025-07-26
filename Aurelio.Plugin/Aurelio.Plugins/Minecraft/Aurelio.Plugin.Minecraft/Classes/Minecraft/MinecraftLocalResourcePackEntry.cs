using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Plugin.Minecraft.Classes.Minecraft;

public class MinecraftLocalResourcePackEntry : ReactiveObject
{
    [Reactive] public string Name { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public Bitmap Icon { get; set; }
    [Reactive] public string Description { get; set; } = string.Empty;
}