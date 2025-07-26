global using static Aurelio.Public.Module.Ui.Overlay;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Aurelio.Plugin.Base;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Plugin.Minecraft.Views;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Plugin.Events;
using Avalonia.Input;
using MinecraftLaunch;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;

namespace Aurelio.Plugin.Minecraft;

public partial class Main : IPlugin
{
    public string Id { get; set; } = "Aurelio.Plugin.Minecraft";
    public string Name { get; set; } = "Minecraft Plugin";
    public string Author { get; set; } = "Yeppioo (yeppioo.vip)";
    public string Description { get; set; } = "Provides Minecraft support for Aurelio.";

    public object SettingPage
    {
        get => new SettingTabPage();
        set => Console.WriteLine("Setting page is not supported");
    }

    public Version Version { get; set; } = Version.Parse("1.0.0");
    public RequirePluginEntry[] Require { get; set; } = [];

    public int Execute()
    {
        AppEvents.AfterUiLoaded += AppEventsOnAfterUiLoaded;
        AppEvents.BeforeUiLoaded += AppEventsOnBeforeUiLoaded;
        AppEvents.BeforeReadSettings += AppEventsOnBeforeReadSettings;
        AppEvents.SaveSettings += AppEventsOnSaveSettings;
        return 0;
    }

    private void AppEventsOnSaveSettings(object? sender, EventArgs e)
    {
        File.WriteAllText(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yeppioo.Aurelio",
            "Aurelio.Plugin.Minecraft.Setting.Yeppioo"), MinecraftPluginData.MinecraftPluginSettingEntry.AsJson());
    }

    private void AppEventsOnBeforeReadSettings(object? sender, EventArgs e)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yeppioo.Aurelio",
            "Aurelio.Plugin.Minecraft.Setting.Yeppioo");
        if (!File.Exists(path))
            File.WriteAllText(path, new MinecraftPluginSettingEntry().AsJson());
        MinecraftPluginData.MinecraftPluginSettingEntry =
            JsonConvert.DeserializeObject<MinecraftPluginSettingEntry>(File.ReadAllText(path));
        Service.UpdateSetting.Main();
        Service.AggregateSearch.Main();
    }

    private void AppEventsOnBeforeUiLoaded(object? sender, EventArgs e)
    {
        HttpUtil.Initialize();
        DownloadMirrorManager.MaxThread = 128;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }


    private void AppEventsOnAfterUiLoaded(object? sender, EventArgs e)
    {
        MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.CollectionChanged +=
            (_, _) => PublicEvents.OnUpdateAggregateSearchEntries();
        _ = MinecraftInstancesHandler.Load(MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftFolderEntries
            .Select(x => x.Path).ToArray());
    }
}