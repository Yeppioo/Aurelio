using System.IO;

namespace Aurelio.Public.Const;

public static class ConfigPath
{
    private static readonly string _sessionTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

    public static string UserDataRootPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Yeppioo.Aurelio");
    public static string TempFolderPath => Path.Combine(UserDataRootPath, "Yeppioo.Temp");

    public static string SettingDataPath => Path.Combine(UserDataRootPath, "Aurelio.Setting.Yeppioo");
    public static string AppPathDataPath => Path.Combine(UserDataRootPath, "Aurelio.AppPath.Yeppioo");
    public static string PluginFolderPath => Path.Combine(UserDataRootPath, "Yeppioo.Plugins");
    public static string PluginUnzipFolderPath => Path.Combine(PluginFolderPath, "Yeppioo.UnzipPlugins");
    public static string PluginTempFolderPath => Path.Combine(PluginUnzipFolderPath, _sessionTimestamp);
}