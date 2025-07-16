using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.IO.Http.Minecraft.Skin;

public class MicrosoftSkinFetcher(string uuid)
{
    public const string BaseApi = "https://sessionserver.mojang.com/session/minecraft/profile/";

    public async ValueTask<byte[]> GetSkinAsync()
    {
        var json = await $"{BaseApi}{uuid}".GetStringAsync();
        var sinon =
            Encoding.UTF8.GetString(Convert.FromBase64String(JsonConvert.DeserializeObject<Models.AccountSkinModel>(json)!
                .Properties.First().Value));
        var url = JsonConvert.DeserializeObject<Models.SkinMoreInfo>(sinon)!.Textures.Skin.Url;
        return await url.GetBytesAsync();
    }
}