using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurelio.Public.Module.App.Services;

public class Update
{
    private const string GITHUB_API = "https://api.github.com/repos/Yeppioo/Aurelio/releases?per_page=1";
    private const string DOWNLOAD_BASE_URL = "https://github.com/Yeppioo/Aurelio/releases/download/auto-publish/";

    public static async Task<UpdateInfo> CheckUpdate()
    {
        var json = await GITHUB_API
            .WithHeader("User-Agent", "Aurelio-App")
            .GetStringAsync();

        var latest = JArray.Parse(json)[0];

        var info = new UpdateInfo
        {
            Body = latest["body"].ToString(),
            ReleaseTime = DateTime.Parse(latest["published_at"].ToString()),
            NewVersion = latest["name"].ToString(),
            IsNeedUpdate = latest["name"].ToString() != Data.Instance.Version
        };

        return info;
    }
}