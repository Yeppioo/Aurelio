using System.Linq;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Const;
using Aurelio.Public.Module.Value;
using MinecraftLaunch.Components.Authenticator;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App.Init.Config;

public class Update
{
    public static void Main()
    {
        if (Data.SettingEntry.MinecraftAccounts.Count == 0)
        {
            Data.SettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
            {
                Name = "Steve", AccountType = Enum.Setting.AccountType.Offline,
                AddTime = DateTime.Now, UUID = Calculator.NameToMcOfflineUUID("Steve").ToString(),
                Data = JsonConvert.SerializeObject(
                    new OfflineAuthenticator().Authenticate("Steve", Calculator.NameToMcOfflineUUID("Steve")))
            });
            Data.SettingEntry.UsingMinecraftAccount = Data.SettingEntry.MinecraftAccounts[0];
        }

        if (Data.SettingEntry.UsingMinecraftAccount == null ||
            !Data.SettingEntry.MinecraftAccounts.Contains(Data.SettingEntry.UsingMinecraftAccount))
        {
            Data.SettingEntry.UsingMinecraftAccount = Data.SettingEntry.MinecraftAccounts[0];
        }

        AppMethod.SaveSetting();
    }
}