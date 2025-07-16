using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.IO.Http.Minecraft.Skin;

public class Models
{
    public record AccountSkinModel
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("properties")] public List<SkinInfo> Properties { get; set; }
    }

    public record SkinInfo
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("value")] public string Value { get; set; }
    }

    public record SkinMoreInfo
    {
        [JsonProperty("timestamp")] public long TimeStamp { get; set; }

        [JsonProperty("profileId")] public string ProfileId { get; set; }

        [JsonProperty("profileName")] public string ProfileName { get; set; }

        [JsonProperty("textures")] public Textures Textures { get; set; }
    }

    public record Textures
    {
        [JsonProperty("SKIN")] public SKIN Skin { get; set; }

        [JsonProperty("CAPE")] public CAPE Cape { get; set; }
    }

    public record SKIN
    {
        [JsonProperty("url")] public string Url { get; set; }
    }

    public record CAPE
    {
        [JsonProperty("url")] public string Url { get; set; }
    }
}