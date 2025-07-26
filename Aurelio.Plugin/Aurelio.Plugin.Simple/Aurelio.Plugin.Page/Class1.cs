using Aurelio.Plugin.Base;

namespace Aurelio.Plugin.Page;

public class Class1 : IPlugin
{
    public string Id { get; set; } = "Aurelio.Plugin.Simple.Page";
    public string Name { get; set; } = "Page";
    public string Author { get; set; } = "Aurelio";
    public string Description { get; set; } = "Page plugin";
    public object SettingPage { get; set; } = new SimplePage();
    public Version Version { get; set; } = Version.Parse("1.0.0");
    public RequirePluginEntry[] Require { get; set; } = [];
    public int Execute()
    {
        return 0;
    }
}