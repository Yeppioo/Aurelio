using Aurelio.Plugin.Base;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Avalonia.Threading;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace Aurelio.Public.Module.App.Services;

public class CheckPluginUpdate
{
    private const string NUGET_API_BASE = "https://api.nuget.org/v3-flatcontainer";

    public static void Main(LoadedPluginEntry[] plugins)
    {
        foreach (var plugin in plugins)
            if (plugin.Plugin.PackageInfo is NugetPackage)
                _ = CheckUpdate(plugin);
    }

    public static async Task CheckUpdate(LoadedPluginEntry plugin)
    {
        if (plugin.Plugin.PackageInfo is not NugetPackage package)
            return;

        try
        {
            // 设置检查状态
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                plugin.NugetPackageUpdateState = MainLang.CheckingUpdate;
            });

            // 获取包的所有版本
            var versionsUrl = $"{NUGET_API_BASE}/{package.Id.ToLowerInvariant()}/index.json";
            var response = await versionsUrl
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithHeader("User-Agent", "Aurelio-App")
                .GetStringAsync();

            var versionsJson = JObject.Parse(response);
            var versions = versionsJson["versions"]?.ToObject<string[]>();

            if (versions == null || versions.Length == 0)
            {
                await UpdatePluginState(plugin, MainLang.PackageInfoUnavailable, false);
                return;
            }

            // 获取最新版本
            var latestVersion = GetLatestVersion(versions);
            var currentVersion = plugin.Plugin.Version;

            // 比较版本
            if (IsNewerVersion(latestVersion, currentVersion))
            {
                await UpdatePluginState(plugin, $"{MainLang.UpdateAvailable}: v{latestVersion}", true, latestVersion.ToString());
            }
            else
            {
                await UpdatePluginState(plugin, MainLang.CurrentlyTheLatestVersion, false);
            }
        }
        catch (FlurlHttpException ex)
        {
            Logger.Error($"检查插件 {plugin.Plugin.Name} 更新时发生网络错误: {ex.Message}");
            await UpdatePluginState(plugin, MainLang.CheckUpdateFail, false);
        }
        catch (Exception ex)
        {
            Logger.Error($"检查插件 {plugin.Plugin.Name} 更新时发生错误: {ex.Message}");
            await UpdatePluginState(plugin, MainLang.CheckUpdateFail, false);
        }
    }

    private static async Task UpdatePluginState(LoadedPluginEntry plugin, string state, bool hasUpdate = false, string? latestVersion = null)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            plugin.NugetPackageUpdateState = state;
            plugin.HasUpdate = hasUpdate;
            plugin.LatestVersion = latestVersion;
        });
    }

    private static Version GetLatestVersion(string[] versions)
    {
        Version latestVersion = null;

        foreach (var versionString in versions)
        {
            if (Version.TryParse(versionString, out var version))
            {
                if (latestVersion == null || version > latestVersion)
                {
                    latestVersion = version;
                }
            }
        }

        return latestVersion ?? new Version("0.0.0");
    }

    private static bool IsNewerVersion(Version latestVersion, Version currentVersion)
    {
        return latestVersion > currentVersion;
    }
}