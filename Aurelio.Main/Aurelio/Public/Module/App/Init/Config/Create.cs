using System.Collections.Generic;
using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Newtonsoft.Json;

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
        if (!File.Exists(ConfigPath.ProjectIndexPath))
        {
            File.WriteAllText(ConfigPath.ProjectIndexPath, JsonConvert.SerializeObject(new List<ProjectIndexEntry>()
            {
                new() { Title = MainLang.ExampleProject }
            }));
        }
    }

    public static void Folder()
    {
        IO.Local.Setter.TryCreateFolder(ConfigPath.UserDataRootPath);
        IO.Local.Setter.TryCreateFolder(ConfigPath.ProjectFolderPath);
        IO.Local.Setter.TryCreateFolder(ConfigPath.ProjectDataFolderPath);
    }
}