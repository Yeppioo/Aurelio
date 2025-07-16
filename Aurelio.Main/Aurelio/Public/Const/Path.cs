using System.IO;

namespace Aurelio.Public.Const;

public static class ConfigPath
{
    public static string UserDataRootPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Yeppioo.Aurelio");
    public static string TempFolderPath => Path.Combine(UserDataRootPath, "Yeppioo.Temp");

    public static string SettingDataPath => Path.Combine(UserDataRootPath, "Aurelio.Setting.Yeppioo");
    public static string AppPathDataPath => Path.Combine(UserDataRootPath, "Aurelio.AppPath.Yeppioo");
}