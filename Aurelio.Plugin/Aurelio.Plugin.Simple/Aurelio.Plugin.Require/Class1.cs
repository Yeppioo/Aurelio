using Aurelio.Plugin.Base;

namespace Aurelio.Plugin.Require;

public class Class1 : IPlugin
{
    public string Id => "Aurelio.Plugin.Simple.Require";
    public string Name => "Require";
    public string Author => "Aurelio";
    public string Description => "Require plugin";
    public object SettingPage => null;
    public Version Version => Version.Parse("1.0.0");

    public RequirePluginEntry[] Require { get; } =
    [
        new()
        {
            Id = "Aurelio.Plugin.Simple.BeRequire",
            VersionRange = [Version.Parse("1.0.0")],
            RequireMethod = RequireMethod.GreaterThanOrEqual
        }
    ];

    public int Execute()
    {
        return 0;
    }
}