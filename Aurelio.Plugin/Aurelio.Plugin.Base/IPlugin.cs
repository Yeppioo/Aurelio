namespace Aurelio.Plugin.Base;

public interface IPlugin
{
    string Id { get; set; }
    string Name { get; set; }
    string Author { get; set; }
    string Description { get; set; }
    object SettingPage { get; set; }
    Version Version { get; set; }
    RequirePluginEntry[] Require { get; set; }
    object? PackageInfo { get; set; }

    int Execute();
}