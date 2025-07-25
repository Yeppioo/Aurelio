using System.IO;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Module.IO.Local;

namespace Aurelio.Public.Module.App.Init.Config;

public abstract class Create
{
    public static void Main()
    {
        Folder();
        Data();
    }

    public static void Data()
    {
        if (!File.Exists(ConfigPath.SettingDataPath))
            File.WriteAllText(ConfigPath.SettingDataPath, new SettingEntry().AsJson());
    }

    public static void Folder()
    {
        Setter.TryCreateFolder(ConfigPath.UserDataRootPath);
        Setter.TryCreateFolder(ConfigPath.PluginFolderPath);
        Setter.TryCreateFolder(ConfigPath.TempFolderPath);
    }
}