using Aurelio.Plugin.Base;

namespace Aurelio.Plugin.BeRequire;

public class Class1 : IPlugin
{
    public string Id { get; set; } = "Aurelio.Plugin.Simple.BeRequire";
    public string Name { get; set; } = "BeRequire";
    public string Author { get; set; } = "Aurelio";
    public string Description { get; set; } = "BeRequire";
    public object SettingPage { get; set; } = null;
    public Version Version { get; set; } = Version.Parse("1.0.0");
    public RequirePluginEntry[] Require { get; set; } = [];

    public IPackageInfo? PackageInfo { get; set; }

    public int Execute()
    {
        return 0;
    }
}