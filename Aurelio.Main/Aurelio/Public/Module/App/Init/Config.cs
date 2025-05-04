using Aurelio.Public.Const;

namespace Aurelio.Public.Module.App.Init;

public class Config
{
    public static void CreateFolder()
    {
        Module.IO.Setter.TryCreateFolder(ConfigPath.RootFolderPath);
        Module.IO.Setter.TryCreateFolder(ConfigPath.PluginFolderPath);
    }
}