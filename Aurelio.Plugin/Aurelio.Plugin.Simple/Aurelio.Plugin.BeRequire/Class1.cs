using Aurelio.Plugin.Base;

namespace Aurelio.Plugin.BeRequire;

public class Class1 : IPlugin
{
    public string Id => "Aurelio.Plugin.Simple.BeRequire";
    public string Name => "BeRequire";
    public string Author => "Aurelio";
    public string Description => "BeRequire";
    public object SettingPage => null;
    public Version Version => Version.Parse("1.0.0");
    public RequirePluginEntry[] Require { get; } = [];
    public int ExecuteBeforeReadSettings()
    {
        Console.WriteLine("BeRequire, ExecuteBeforeReadSettings");
        return 0;
    }

    public int ExecuteBeforeUiLoaded()
    {
        Console.WriteLine("BeRequire, ExecuteBeforeUiLoaded");
        return 0;
    }

    public int ExecuteAfterUiLoaded()
    {
        Console.WriteLine("BeRequire, ExecuteAfterUiLoaded");
        return 0;
    }
}