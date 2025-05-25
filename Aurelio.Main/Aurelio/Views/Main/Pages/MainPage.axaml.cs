using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO.Http.Api;
using Aurelio.Public.Module.IO.Http.Api.Aurelio;
using Aurelio.Public.Module.Ui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace Aurelio.Views.Main.Pages;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        BindEvents();
    }

    private void BindEvents()
    {
        UserBorder.PointerPressed += async (_, e) =>
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
            if (Data.Instance.AccountEntry.Tag == "no-login")
            {
                await LoginPage();
            }
        };
    }

    private static async Task LoginPage(string a = null, string p = null)
    {
        while (true)
        {
            var accountText = new TextBox { Watermark = MainLang.Account, Text = a };
            var passwordText = new TextBox { Watermark = MainLang.Password, Text = p };

            async Task<ContentDialogResult> ShowLoginInput()
            {
                var c = new StackPanel() { Spacing = 10, Children = { accountText, passwordText } };
                return await Shower.ShowDialogAsync(MainLang.Login, p_content: c, b_cancel: MainLang.Cancel,
                    b_primary: MainLang.OK, b_secondary: MainLang.RegisterNewAccount);
            }

            var result = await ShowLoginInput();
            if (result == ContentDialogResult.None) return;

            if (result == ContentDialogResult.Primary)
            {
                var account = accountText.Text;
                var password = passwordText.Text;
                var tuple = await Login.Send(account, password);
                if (tuple.code != 200)
                {
                    Shower.Notice($"{MainLang.LoginFail}: {tuple.data.message}", NotificationType.Error);
                    a = accountText.Text;
                    p = passwordText.Text;
                    continue;
                }
                else
                {
                    var exits = Data.SettingEntry.AccountRecordEntries
                        .FirstOrDefault(x => x.Account == account);
                    if (exits != null)
                    {
                        exits.Password = password;
                        Data.SettingEntry.CurrentAccount = exits;
                    }
                    else
                    {
                        Data.SettingEntry.AccountRecordEntries.Add(new AccountRecordEntry(account, password));
                        Data.SettingEntry.CurrentAccount = Data.SettingEntry.AccountRecordEntries.LastOrDefault();
                    }
                    Data.Instance.AccountEntry = new AccountEntry
                    {
                        Avatar = tuple.data.data.avatarUrl,
                        Email = tuple.data.data.email,
                        Username = tuple.data.data.username,
                        Password = passwordText.Text
                    };
                    AppMethod.SaveSetting();
                }
            }

            break;
        }
    }
}