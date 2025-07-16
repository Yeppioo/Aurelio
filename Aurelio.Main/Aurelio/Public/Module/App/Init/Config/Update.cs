using System.Linq;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Module.Operate;
using Aurelio.Public.Module.Value;
using DynamicData;
using MinecraftLaunch.Components.Authenticator;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App.Init.Config;

public class Update
{
    public static void Main()
    {
        IO.Local.Setter.ClearFolder(ConfigPath.TempFolderPath);
        
        if (Data.SettingEntry.MinecraftAccounts.Count == 0)
        {
            Data.SettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
            {
                Name = "Steve", AccountType = Setting.AccountType.Offline,
                AddTime = DateTime.Now, UUID = Calculator.NameToMcOfflineUUID("Steve").ToString(),
                Data = JsonConvert.SerializeObject(
                    new OfflineAuthenticator().Authenticate("Steve", Calculator.NameToMcOfflineUUID("Steve")))
            });
            Data.SettingEntry.UsingMinecraftAccount = Data.SettingEntry.MinecraftAccounts[0];
        }

        if (Data.SettingEntry.UsingMinecraftAccount == null ||
            !Data.SettingEntry.MinecraftAccounts.Contains(Data.SettingEntry.UsingMinecraftAccount))
            Data.SettingEntry.UsingMinecraftAccount = Data.SettingEntry.MinecraftAccounts[0];

        Data.SettingEntry.JavaRuntimes.RemoveMany(Data.SettingEntry.JavaRuntimes
            .Where(x => x.JavaVersion == "auto"));
        JavaRuntime.VerifyList();
        AppMethod.SaveSetting();
    }
}