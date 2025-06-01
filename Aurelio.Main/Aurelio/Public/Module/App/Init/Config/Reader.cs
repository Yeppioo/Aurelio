using System.IO;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Const;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App.Init.Config;

public abstract class Reader
{
    public static void Main()
    {
        Data.SettingEntry =
            JsonConvert.DeserializeObject<SettingEntry>(File.ReadAllText(ConfigPath.SettingDataPath));
    }
}