using System.Linq;
using Aurelio.Public.Const;

namespace Aurelio.Public.Module.App.Init.Config;

public class Update
{
    public static void Main()
    {
        if(Data.SettingEntry == null) return;
        if (Data.SettingEntry.CurrentAccount == null && Data.SettingEntry.AccountRecordEntries.Count > 0)
        {
            Data.SettingEntry.CurrentAccount = Data.SettingEntry.AccountRecordEntries.FirstOrDefault();
        }
    }
}