using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Entries.Page;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Const;
using DynamicData;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App.Init.Config;

public abstract class Reader
{
    public static void Main()
    {
        Data.SettingEntry =
            JsonConvert.DeserializeObject<SettingEntry>(File.ReadAllText(ConfigPath.SettingDataPath));
        UiProperty.RecentOpens.AddRange(JsonConvert.DeserializeObject<ObservableCollection<RecentPageEntry>>
            (File.ReadAllText(ConfigPath.RecentOpenDataPath)) ?? []);
    }
}