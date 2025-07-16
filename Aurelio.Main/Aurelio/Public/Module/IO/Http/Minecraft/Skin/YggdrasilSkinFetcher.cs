using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.IO.Http.Minecraft.Skin;

public class YggdrasilSkinFetcher(string url, string uuid)
{
    public readonly string BaseApi = $"{url}/sessionserver/session/minecraft/profile/{uuid.Replace("-", string.Empty)}";

    public async ValueTask<byte[]> GetSkinAsync()
    {
        var json = await BaseApi.GetStringAsync();
        var skin = Encoding.UTF8.GetString(Convert
            .FromBase64String(JsonConvert
                .DeserializeObject<Models.AccountSkinModel>(json)!
                .Properties.First().Value));

        var skinUrl = JsonConvert
            .DeserializeObject<Models.SkinMoreInfo>(skin)!.Textures.Skin.Url;
        return await skinUrl.GetBytesAsync();
    }
}