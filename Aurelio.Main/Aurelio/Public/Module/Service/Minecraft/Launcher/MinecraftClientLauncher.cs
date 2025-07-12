using System.Threading;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Base.Models.Authentication;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Extensions;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.Service.Minecraft.Launcher;

public class MinecraftClientLauncher
{
    public static async Task Launch(RecordMinecraftEntry entry)
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        MinecraftLaunchSettingEntry setting;

        try
        {
            setting = Calculator.CalcMinecraftInstanceSetting(entry);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e.Message.StartsWith("No suitable version of Java found to start this game"))
            {
                _ = ShowDialogAsync(MainLang.LaunchFail, MainLang.CannotFindJavaTip
                        .Replace("{mcVer}", entry.MlEntry.Version.VersionId)
                        .Replace("{javaVer}", entry.MlEntry.GetAppropriateJavaVersion().ToString()),
                    b_primary: MainLang.Ok);
                return;
            }

            throw;
        }

        Notice($"{MainLang.Launch}: {entry.Id}");
        var task = Tasking.CreateTask($"{entry.ParentMinecraftFolder.Name}/{entry.Id}");
        new TaskEntry(MainLang.CheckLaunchArg).AddIn(task);
        var accountTask = new TaskEntry(MainLang.RefreshAccountToken).AddIn(task);
        var buildTask = new TaskEntry(MainLang.BuildLaunchConfig).AddIn(task);
        var launchTask = new TaskEntry(MainLang.LaunchMinecraftProcess).AddIn(task);
        task.NextSubTask();
        task.NextSubTask();

        Account? account = null!;
        switch (Data.SettingEntry.UsingMinecraftAccount.AccountType)
        {
            case Setting.AccountType.Offline:
                if (!string.IsNullOrWhiteSpace(Data.SettingEntry.UsingMinecraftAccount.Name))
                {
                    account = JsonConvert.DeserializeObject<OfflineAccount>(
                        Data.SettingEntry.UsingMinecraftAccount.Data!);
                }
                else
                {
                    Notice(MainLang.AccountError, NotificationType.Error);
                    task.FinishWithError();
                    return;
                }

                break;
            case Setting.AccountType.Microsoft:
                var profile =
                    JsonConvert.DeserializeObject<MicrosoftAccount>(Data.SettingEntry.UsingMinecraftAccount.Data!);
                MicrosoftAuthenticator authenticator2 = new(Config.AzureClientId);
                try
                {
                    account = await authenticator2.RefreshAsync(profile, token);
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.LoginFail, ex);
                    task.FinishWithError();
                    return;
                }

                break;
            case Setting.AccountType.ThirdParty:
                account = JsonConvert.DeserializeObject<YggdrasilAccount>(Data.SettingEntry.UsingMinecraftAccount
                    .Data!);
                break;
        }

        if (task.IsCancelRequest)
        {
            Notice($"{MainLang.Canceled}: {MainLang.Launch} - {entry.Id}", NotificationType.Success);
            task.CancelFinish();
            return;
        }

        if (account == null)
        {
            Notice(MainLang.AccountError, NotificationType.Error);
            task.FinishWithError();
        }
    }
}