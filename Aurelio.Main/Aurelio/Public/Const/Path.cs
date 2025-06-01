using System;
using System.IO;

namespace Aurelio.Public.Const;

public static class ConfigPath
{
    public static string UserDataRootPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Yep.Aurelio");
    public static string SettingDataPath => Path.Combine(UserDataRootPath,"Aurelio.Setting.Yep");
    public static string AppPathDataPath => Path.Combine(UserDataRootPath,"Aurelio.AppPath.Yep");
    public static string ProjectFolderPath => Path.Combine(UserDataRootPath,"Aurelio.Project");
    public static string RecentOpenDataPath => Path.Combine(UserDataRootPath,"Aurelio.RecentOpen.Yep");
}