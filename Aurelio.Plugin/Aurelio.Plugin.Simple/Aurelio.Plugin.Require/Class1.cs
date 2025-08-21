using Aurelio.Plugin.Base;

namespace Aurelio.Plugin.Require;

public class Class1 : IPlugin
{
    public string Id { get; set; } = "Aurelio.Plugin.Simple.Require";
    public string Name { get; set; } = "Require";
    public string Author { get; set; } = "Aurelio";
    public string Description { get; set; } = "Require plugin";
    public object SettingPage { get; set; } = null;
    public Version Version { get; set; } = Version.Parse("1.0.0");

    public RequirePluginEntry[] Require { get; set; } =
    [
        new()
        {
            Id = "Aurelio.Plugin.Simple.BeRequire",
            VersionRange = [Version.Parse("1.0.0")],
            RequireMethod = RequireMethod.GreaterThanOrEqual
        }
    ];

    public IPackageInfo? PackageInfo { get; set; }

    public int Execute()
    {
        return 0;
    }
}