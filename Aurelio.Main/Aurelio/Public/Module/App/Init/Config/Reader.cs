using System.Collections.Generic;
using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App.Init.Config;

public abstract class Reader
{
    public static void Main()
    {
        JsonConvert.DeserializeObject<List<ProjectIndexEntry>>
            (File.ReadAllText(ConfigPath.ProjectIndexPath)).ForEach(Data.ProjectIndexEntries.Add);
    }
}