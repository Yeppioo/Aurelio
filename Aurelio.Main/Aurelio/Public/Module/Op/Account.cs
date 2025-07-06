using System.Collections.Generic;
using System.Linq;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.Value;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Authentication;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Skin.Class.Fetchers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurelio.Public.Module.Op;

public class Account
{
    public static async Task AddByUi(Control sender)
    {
        var comboBox = new ComboBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        comboBox.Items.Add(MainLang.OfflineLogin);
        comboBox.Items.Add(MainLang.MicrosoftLogin);
        comboBox.Items.Add(MainLang.ThirdPartyLogin);
        comboBox.SelectedIndex = 0;
        ContentDialog dialog = new()
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            Title = MainLang.SelectAccountType,
            PrimaryButtonText = MainLang.Ok,
            CloseButtonText = MainLang.Cancel,
            DefaultButton = ContentDialogButton.Primary,
            Content = comboBox
        };
        var dialogResult = await dialog.ShowAsync(TopLevel.GetTopLevel(sender));
        if (dialogResult == ContentDialogResult.Primary)
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    var textBox = new TextBox
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        TextWrapping = TextWrapping.Wrap, Watermark = MainLang.AccountName
                    };
                    var uuidTextBox = new TextBox
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        TextWrapping = TextWrapping.Wrap, Watermark = MainLang.AddNewAccountUuid
                    };
                    ContentDialog offlineDialog = new()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Title = MainLang.AddNewAccount,
                        PrimaryButtonText = MainLang.Ok,
                        CloseButtonText = MainLang.Cancel,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = new StackPanel()
                        {
                            Spacing = 10,
                            Children = { textBox, uuidTextBox }
                        }
                    };
                    var dialogResult1 = await offlineDialog.ShowAsync(TopLevel.GetTopLevel(sender));
                    if (dialogResult1 == ContentDialogResult.Primary)
                    {
                        if (!string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            var now = DateTime.Now;
                            OfflineAuthenticator authenticator3 = new();
                            try
                            {
                                Data.SettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                                {
                                    AccountType = Setting.AccountType.Offline,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(
                                        authenticator3.Authenticate(textBox.Text,
                                            uuidTextBox.Text == null ? Guid.NewGuid() : Guid.Parse(uuidTextBox.Text))),
                                    Name = textBox.Text
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                Notice(MainLang.OperateFailed, NotificationType.Error);
                            }

                            AppMethod.SaveSetting();
                        }
                        else
                        {
                            Notice(MainLang.AccountNameCannotBeNull, NotificationType.Error);
                        }
                    }

                    AppMethod.SaveSetting();
                    break;
                case 1:
                    var verificationUrl = string.Empty;
                    var verificationCode = string.Empty;
                    MicrosoftAccount userProfile;
                    var textBlock = new TextBlock
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        TextWrapping = TextWrapping.Wrap, Text = MainLang.Loading,
                        HorizontalAlignment = HorizontalAlignment.Center, FontSize = 16
                    };
                    ContentDialog microsoftDialog = new()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Title = MainLang.VerificationCode,
                        PrimaryButtonText = MainLang.CopyCodeAndOPenBrowser,
                        SecondaryButtonText = MainLang.ManualOpen,
                        CloseButtonText = MainLang.Cancel,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = textBlock,
                        IsPrimaryButtonEnabled = false,
                        IsSecondaryButtonEnabled = false
                    };
                    MicrosoftAuthenticator authenticator = new(Config.AzureClientId);
                    microsoftDialog.PrimaryButtonClick += async (_, _) =>
                    {
                        var clipboard = TopLevel.GetTopLevel(sender)?.Clipboard;
                        await clipboard.SetTextAsync(textBlock.Text);
                        var launcher = TopLevel.GetTopLevel(sender).Launcher;
                        await launcher.LaunchUriAsync(new Uri(verificationUrl));
                        Notice(MainLang.WaitForMicrosoftVerification);
                    };
                    microsoftDialog.SecondaryButtonClick += (_, _) =>
                    {
                        var urlBox = new TextBox
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            TextWrapping = TextWrapping.Wrap, IsReadOnly = true
                        };
                        var tip = new TextBlock
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"], FontSize = 14,
                            Text = MainLang.CopyUrlAndManualOpen
                        };
                        var codeBox = new TextBox
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            TextWrapping = TextWrapping.Wrap, IsReadOnly = true, Text = verificationCode
                        };
                        var codeTip = new TextBlock
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"], FontSize = 14,
                            Text = MainLang.VerificationCode
                        };
                        var stackPanel = new StackPanel { Spacing = 10 };
                        stackPanel.Children.Add(tip);
                        stackPanel.Children.Add(urlBox);
                        stackPanel.Children.Add(codeTip);
                        stackPanel.Children.Add(codeBox);
                        ContentDialog urlDialog = new()
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            Title = MainLang.ManualOpen,
                            PrimaryButtonText = MainLang.Ok,
                            DefaultButton = ContentDialogButton.Primary,
                            Content = stackPanel
                        };
                        urlBox.Text = verificationUrl;
                        _ = urlDialog.ShowAsync(TopLevel.GetTopLevel(sender));
                    };
                    _ = microsoftDialog.ShowAsync(TopLevel.GetTopLevel(sender));
                    try
                    {
                        var token = await authenticator.DeviceFlowAuthAsync(device =>
                        {
                            textBlock.Text = device.UserCode;
                            verificationUrl = device.VerificationUrl;
                            verificationCode = device.UserCode;
                            microsoftDialog.IsPrimaryButtonEnabled = true;
                            microsoftDialog.IsSecondaryButtonEnabled = true;
                        });
                        userProfile = await authenticator.AuthenticateAsync(token);
                    }
                    catch (Exception ex)
                    {
                        ShowShortException(MainLang.LoginFail, ex);
                        return;
                    }

                    try
                    {
                        Notice(MainLang.VerifyingAccount);
                        MicrosoftSkinFetcher skinFetcher = new(userProfile.Uuid.ToString());
                        var bytes = await skinFetcher.GetSkinAsync();
                        var now = DateTime.Now;
                        Data.SettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                        {
                            AccountType = Setting.AccountType.Microsoft,
                            AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                            Data = JsonConvert.SerializeObject(userProfile, Formatting.Indented),
                            Name = userProfile.Name,
                            Skin = Converter.BytesToBase64(bytes)
                        });
                        AppMethod.SaveSetting();
                        if (TopLevel.GetTopLevel(sender) is Window window)
                        {
                            window.Activate();
                        }

                        Notice($"{MainLang.LoginSucess}: {userProfile.Name}");
                    }
                    catch (Exception ex)
                    {
                        ShowShortException(MainLang.LoginFail, ex);
                    }

                    AppMethod.SaveSetting();
                    break;
                case 2:
                    await YggdrasilLogin(sender);
                    break;
            }
    }

    public static async Task YggdrasilLogin(Control sender, string server1 = "", string email1 = "",
        string password1 = "")
    {
        var stackPanel = new StackPanel { Spacing = 10, Width = 580 };
        var verificationSeverUrlTextBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Watermark = MainLang.VerificationServer, Text = server1, HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = 500
        };
        var emailTextBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Watermark = MainLang.EmailAddress, Text = email1, HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = 500
        };
        var passwordTextBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Watermark = MainLang.AccountPassword, Text = password1, HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = 500
        };
        stackPanel.Children.Add(verificationSeverUrlTextBox);
        stackPanel.Children.Add(emailTextBox);
        stackPanel.Children.Add(passwordTextBox);
        ContentDialog thirdPartyDialog = new()
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            Title = MainLang.ThirdPartyLogin,
            PrimaryButtonText = MainLang.Ok,
            CloseButtonText = MainLang.Cancel,
            DefaultButton = ContentDialogButton.Primary,
            Content = stackPanel
        };
        var thirdPartyDialogResult = await thirdPartyDialog.ShowAsync(TopLevel.GetTopLevel(sender));
        if (thirdPartyDialogResult == ContentDialogResult.Primary)
        {
            var server = verificationSeverUrlTextBox.Text;
            var email = emailTextBox.Text;
            var password = passwordTextBox.Text;
            var reInput = false;
            if (string.IsNullOrWhiteSpace(server) && string.IsNullOrWhiteSpace(server))
            {
                Notice(MainLang.YggdrasilServerUrlIsEmpty, NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(email))
            {
                Notice(MainLang.YggdrasilEmailIsEmpty, NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(password))
            {
                Notice(MainLang.YggdrasilPasswordIsEmpty, NotificationType.Error);
                reInput = true;
            }

            if (reInput)
            {
                await YggdrasilLogin(sender, server, email, password);
            }
            else
            {
                IEnumerable<YggdrasilAccount> yggdrasilAccounts;
                try
                {
                    YggdrasilAuthenticator authenticator = new(server, email, password);
                    Notice(MainLang.VerifyingAccount);
                    yggdrasilAccounts = (await authenticator.AuthenticateAsync()).ToList();
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.LoginFail, ex);
                    return;
                }

                try
                {
                    foreach (var account in yggdrasilAccounts)
                    {
                        var now = DateTime.Now;
                        try
                        {
                            YggdrasilSkinFetcher skinFetcher = new(account.YggdrasilServerUrl, account.Uuid.ToString());
                            var bytes = await skinFetcher.GetSkinAsync();
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Data.SettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                                {
                                    AccountType = Setting.AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name,
                                    Skin = Converter.BytesToBase64(bytes)
                                });
                            });
                        }
                        catch
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Data.SettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                                {
                                    AccountType = Setting.AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name
                                });
                            });
                        }
                    }

                    AppMethod.SaveSetting();
                    if (TopLevel.GetTopLevel(sender) is Window window)
                    {
                        window.Activate();
                    }
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.LoginFail, ex);
                }
            }
        }
    }

    public static void RemoveSelected()
    {
        var item = Data.SettingEntry.UsingMinecraftAccount;
        if (item == null) return;
        Data.SettingEntry.MinecraftAccounts.Remove(item);
        if (Data.SettingEntry.MinecraftAccounts.Count == 0)
        {
            var account = new RecordMinecraftAccount
            {
                Name = "Steve", AccountType = Setting.AccountType.Offline,
                AddTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Data = JsonConvert.SerializeObject(new OfflineAuthenticator().Authenticate("Steve"))
            };
            Data.SettingEntry.MinecraftAccounts.Add(account);
            Data.SettingEntry.UsingMinecraftAccount = account;
        }
        else
        {
            Data.SettingEntry.UsingMinecraftAccount = Data.SettingEntry.MinecraftAccounts[0];
        }

        AppMethod.SaveSetting();
    }

    public static async Task RefreshSelectedMicrosoftAccountSkin()
    {
        if (Data.SettingEntry.UsingMinecraftAccount is not { AccountType: Setting.AccountType.Microsoft }) return;
        if (Data.SettingEntry.UsingMinecraftAccount.Data != null)
        {
            var obj = JObject.Parse(Data.SettingEntry.UsingMinecraftAccount.Data);
            var uuid = obj["Uuid"].ToString();
            MicrosoftSkinFetcher skinFetcher = new(uuid);
            var skin = await skinFetcher.GetSkinAsync();
            Data.SettingEntry.UsingMinecraftAccount.UpdateSkin(skin);
        }
    }
}