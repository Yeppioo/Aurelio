using Aurelio.Public.Classes.Interfaces;
using Newtonsoft.Json;

namespace Aurelio.Public.Classes.Setting;

public class LaunchPageEntry
{
    public string Id { get; init; }
    [JsonIgnore]
    public string Header { get; set; }
    public string Tag { get; set; } // 每个窗口中只能存在一个相同标签
    [JsonIgnore]
    public IAurelioTabPage Page { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is not LaunchPageEntry entry) return false;
        return entry.Id == Id;
    }
}