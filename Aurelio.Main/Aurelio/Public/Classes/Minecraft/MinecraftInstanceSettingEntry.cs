using Aurelio.Public.Classes.Enum.Minecraft;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Minecraft;

public class MinecraftInstanceSettingEntry : ReactiveObject
{
    [Reactive] [JsonProperty] public DateTime LastPlayed { get; set; } = DateTime.MinValue;
    [Reactive] [JsonProperty] public MinecraftInstanceIconType IconType { get; set; } = MinecraftInstanceIconType.Auto;
    [Reactive] [JsonProperty] public double MemoryLimit { get; set; } = -1;
    [Reactive] [JsonProperty] public int EnableIndependentMinecraft { get; set; }
    [Reactive] [JsonProperty] public string AutoJoinServerAddress { get; set; }
    [Reactive] [JsonProperty] public RecordJavaRuntime JavaRuntime { get; set; } = new()
    {
        JavaVersion = "global"
    };
    [Reactive] [JsonProperty] public string IconData { get; set; }
}

