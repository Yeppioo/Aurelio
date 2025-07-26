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
    public int Execute()
    {
        return 0;
    }
}