global using static Aurelio.Public.Module.Ui.Overlay;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Aurelio.Plugin.Base;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Plugin.Minecraft.Views;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;
using Aurelio.Public.Module.Plugin.Events;
using Avalonia.Input;
using MinecraftLaunch;
using MinecraftLaunch.Utilities;

namespace Aurelio.Plugin.Minecraft;

public partial class Main : IPlugin
{
    public string Id => "Aurelio.Plugin.Minecraft";
    public string Name => "Minecraft Plugin";
    public string Author => "Yeppioo (yeppioo.vip)";
    public string Description => "Provides Minecraft support for Aurelio.";
    public object SettingPage => new SettingTabPage();
    public Version Version { get; } = Version.Parse("1.0.0");
    public RequirePluginEntry[] Require { get; } = [];

    public int Execute()
    {
        AppEvents.AfterUiLoaded += AppEventsOnAfterUiLoaded;
        AppEvents.BeforeUiLoaded += AppEventsOnBeforeUiLoaded;
        return 0;
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