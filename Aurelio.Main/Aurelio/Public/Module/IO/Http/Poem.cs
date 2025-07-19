using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace Aurelio.Public.Module.IO.Http;

public class Poem
{
    public static async Task<string> GetToken()
    {
        try
        {
            var json = await "https://v2.jinrishici.com/token".GetStringAsync();
            var obj = JObject.Parse(json);
            return obj["status"].ToString() != "success" ? null : obj["data"].ToString();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        return null;
    }

    public static async Task<string> GetPoem()
    {
        try
        {
            if (Data.SettingEntry.PoemApiToken.IsNullOrWhiteSpace())
            {
                var token = await GetToken();
                Data.SettingEntry.PoemApiToken = token;
            }

            if (Data.SettingEntry.PoemApiToken.IsNullOrWhiteSpace()) return null;
            var poemJson = await "https://v2.jinrishici.com/one.json"
                .WithHeader("X-User-Token", Data.SettingEntry.PoemApiToken).GetStringAsync();

            var obj = JObject.Parse(poemJson);
            if (obj["status"]?.ToString() != "success" || obj["data"] == null) return null;
            var content = obj["data"]["content"]?.ToString();
            var dynasty = obj["data"]["origin"]?["dynasty"]?.ToString();
            var author = obj["data"]["origin"]?["author"]?.ToString();

            if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(dynasty) && !string.IsNullOrEmpty(author))
            {
                return $"{content} —— {dynasty} {author}";
            }

            return null;
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        return null;
    }
}