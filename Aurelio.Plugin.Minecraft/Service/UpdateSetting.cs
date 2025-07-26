using Aurelio.Plugin.Minecraft.Classes.Enum;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Operate;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using DynamicData;
using MinecraftLaunch.Components.Authenticator;
using Newtonsoft.Json;

namespace Aurelio.Plugin.Minecraft.Service;

public class UpdateSetting
{
    public static void Main()
    {
        if (MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Count == 0)
        {
            MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
            {
                Name = "Steve", AccountType = Setting.AccountType.Offline,
                AddTime = DateTime.Now, UUID = Calculator.NameToMcOfflineUUID("Steve").ToString(),
                Data = JsonConvert.SerializeObject(
                    new OfflineAuthenticator().Authenticate("Steve", Calculator.NameToMcOfflineUUID("Steve")))
            });
            MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount = MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts[0];
        }

        if (MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount == null ||
            !MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Contains(MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount))
            MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount = MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts[0];

        MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.RemoveMany(MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes
            .Where(x => x.JavaVersion == "auto"));
        JavaRuntime.VerifyList();
    }
}