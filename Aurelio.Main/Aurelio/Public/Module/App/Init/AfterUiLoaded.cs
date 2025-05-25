using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Http.Api.Aurelio;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls.Notifications;

namespace Aurelio.Public.Module.App.Init;

public abstract class AfterUiLoaded
{
    public static void Main()
    {
        _ = LoginAccount();
    }

    private static async Task LoginAccount()
    {
        var account = Data.SettingEntry.CurrentAccount;
        if (account == null) return;
        if (account.Account.IsNullOrWhiteSpace() || account.Password.IsNullOrWhiteSpace()) return;
        Data.Instance.AccountEntry.Username = MainLang.Logining;
        var tuple = await Login.Send(account.Account, account.Password);
        if (tuple.code != 200)
        {
            Shower.Notice($"{MainLang.LoginFail}: {tuple.data.message}", NotificationType.Error);
            Data.Instance.AccountEntry = new AccountEntry
            {
                Avatar = "../../../Public/Assets/user.png", Tag = "no-login", Username = MainLang.NoLogin
            };
        }
        else
        {
            Data.Instance.AccountEntry = new AccountEntry
            {
                Avatar = tuple.data.data.avatarUrl,
                Email = tuple.data.data.email,
                Username = tuple.data.data.username,
                Password = account.Password
            };
            AppMethod.SaveSetting();
        }
    }
}