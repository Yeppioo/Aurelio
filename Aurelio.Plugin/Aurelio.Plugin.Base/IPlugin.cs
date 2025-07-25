namespace Aurelio.Plugin.Base;

public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Author { get; }
    string Description { get; }
    object SettingPage { get; }
    Version Version { get; }
    RequirePluginEntry[] Require { get; }

    int ExecuteBeforeReadSettings();
    int ExecuteBeforeUiLoaded();
    int ExecuteAfterUiLoaded();
}