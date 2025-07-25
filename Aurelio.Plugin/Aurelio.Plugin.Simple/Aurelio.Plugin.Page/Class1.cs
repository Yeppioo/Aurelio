using Aurelio.Plugin.Base;

namespace Aurelio.Plugin.Page;

public class Class1 : IPlugin
{
    public string Id => "Aurelio.Plugin.Simple.Page";
    public string Name => "Page";
    public string Author => "Aurelio";
    public string Description => "Page plugin";
    public object SettingPage => new SimplePage();
    public Version Version { get; } = Version.Parse("1.0.0");
    public RequirePluginEntry[] Require { get; } = [];
    public int ExecuteBeforeReadSettings()
    {
        return 0;
    }

    public int ExecuteBeforeUiLoaded()
    {
        return 0;
    }

    public int ExecuteAfterUiLoaded()
    {
        return 0;
    }
}