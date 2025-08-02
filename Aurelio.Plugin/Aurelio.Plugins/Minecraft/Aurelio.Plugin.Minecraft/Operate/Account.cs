using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Plugin.Minecraft.Http.Minecraft.Skin;
using Aurelio.Plugin.Minecraft.Service.Minecraft;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.Ui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Authentication;
using MinecraftLaunch.Components.Authenticator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurelio.Plugin.Minecraft.Operate;

public class Account
{
    public static async Task AddByUi(Control sender)
    {
        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        comboBox.Items.Add(MainLang.OfflineLogin);
        comboBox.Items.Add(MainLang.MicrosoftLogin);
        comboBox.Items.Add(MainLang.ThirdPartyLogin);
        comboBox.SelectedIndex = 0;
        ContentDialog dialog = new()
        {
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
                        TextWrapping = TextWrapping.Wrap, Watermark = MainLang.AccountName
                    };
                    var uuidTextBox = new TextBox
                    {
                        TextWrapping = TextWrapping.Wrap, Watermark = MainLang.AddNewAccountUuid
                    };
                    ContentDialog offlineDialog = new()
                    {
                        Title = MainLang.AddNewAccount,
                        PrimaryButtonText = MainLang.Ok,
                        CloseButtonText = MainLang.Cancel,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = new StackPanel
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
                                MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                                {
                                    AccountType = Classes.Enum.Setting.AccountType.Offline,
                                    AddTime = now,
                                    Data = JsonConvert.SerializeObject(
                                        authenticator3.Authenticate(textBox.Text,
                                            uuidTextBox.Text == null
                                                ? Calculator.NameToMcOfflineUUID(textBox.Text)
                                                : Guid.Parse(uuidTextBox.Text))),
                                    Name = textBox.Text,
                                    UUID = uuidTextBox.Text == null
                                        ? Calculator.NameToMcOfflineUUID(textBox.Text).ToString()
                                        : Guid.Parse(uuidTextBox.Text).ToString()
                                });
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
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
                        TextWrapping = TextWrapping.Wrap, Text = MainLang.Loading,
                        HorizontalAlignment = HorizontalAlignment.Center, FontSize = 16
                    };
                    ContentDialog microsoftDialog = new()
                    {
                        Title = MainLang.VerificationCode,
                        PrimaryButtonText = MainLang.CopyCodeAndOPenBrowser,
                        SecondaryButtonText = MainLang.ManualOpen,
                        CloseButtonText = MainLang.Cancel,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = textBlock,
                        IsPrimaryButtonEnabled = false,
                        IsSecondaryButtonEnabled = false
                    };
                    MicrosoftAuthenticator authenticator = new(Public.Const.Config.AzureClientId);
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
                            TextWrapping = TextWrapping.Wrap, IsReadOnly = true
                        };
                        var tip = new TextBlock
                        {
                            FontSize = 14,
                            Text = MainLang.CopyUrlAndManualOpen
                        };
                        var codeBox = new TextBox
                        {
                            TextWrapping = TextWrapping.Wrap, IsReadOnly = true, Text = verificationCode
                        };
                        var codeTip = new TextBlock
                        {
                            FontSize = 14,
                            Text = MainLang.VerificationCode
                        };
                        var stackPanel = new StackPanel { Spacing = 10 };
                        stackPanel.Children.Add(tip);
                        stackPanel.Children.Add(urlBox);
                        stackPanel.Children.Add(codeTip);
                        stackPanel.Children.Add(codeBox);
                        ContentDialog urlDialog = new()
                        {
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
                        MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                        {
                            AccountType = Classes.Enum.Setting.AccountType.Microsoft,
                            AddTime = now, UUID = userProfile.Uuid.ToString(),
                            Data = JsonConvert.SerializeObject(userProfile, Formatting.Indented),
                            Name = userProfile.Name,
                            Skin = Public.Module.Value.Converter.BytesToBase64(bytes)
                        });
                        AppMethod.SaveSetting();
                        if (TopLevel.GetTopLevel(sender) is Window window) window.Activate();

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
        var stackPanel = new StackPanel
        {
            Spacing = 15, MaxWidth = 580, HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(10, 0)
        };
        var verificationSeverUrlTextBox = new TextBox
        {
            TextWrapping = TextWrapping.Wrap, MaxWidth = 500,
            Watermark = MainLang.VerificationServer, Text = server1, HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        var emailTextBox = new TextBox
        { 
            TextWrapping = TextWrapping.Wrap, MaxWidth = 500,
            Watermark = MainLang.EmailAddress, Text = email1, HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        var passwordTextBox = new TextBox
        {
            TextWrapping = TextWrapping.Wrap, MaxWidth = 500,
            Watermark = MainLang.AccountPassword, Text = password1, HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        stackPanel.Children.Add(verificationSeverUrlTextBox);
        stackPanel.Children.Add(emailTextBox);
        stackPanel.Children.Add(passwordTextBox);
        ContentDialog thirdPartyDialog = new()
        {
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
                                MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                                {
                                    AccountType = Classes.Enum.Setting.AccountType.ThirdParty,
                                    AddTime = now, UUID = account.Uuid.ToString(),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name,
                                    Skin = Public.Module.Value.Converter.BytesToBase64(bytes)
                                });
                            });
                        }
                        catch
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Add(new RecordMinecraftAccount
                                {
                                    AccountType = Classes.Enum.Setting.AccountType.ThirdParty,
                                    AddTime = now, UUID = account.Uuid.ToString(),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name
                                });
                            });
                        }
                    }

                    AppMethod.SaveSetting();
                    if (TopLevel.GetTopLevel(sender) is Window window) window.Activate();
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
        var item = MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount;
        if (item == null) return;
        MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Remove(item);
        if (MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Count == 0)
        {
            var account = new RecordMinecraftAccount
            {
                Name = "Steve", AccountType = Classes.Enum.Setting.AccountType.Offline,
                AddTime = DateTime.Now, UUID = Calculator.NameToMcOfflineUUID("Steve").ToString(),
                Data = JsonConvert.SerializeObject(
                    new OfflineAuthenticator().Authenticate("Steve", Calculator.NameToMcOfflineUUID("Steve")))
            };
            MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts.Add(account);
            MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount = account;
        }
        else
        {
            MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount = MinecraftPluginData.MinecraftPluginSettingEntry.MinecraftAccounts[0];
        }

        AppMethod.SaveSetting();
    }

    public static async Task RefreshSelectedMicrosoftAccountSkin()
    {
        if (MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount is not { AccountType: Classes.Enum.Setting.AccountType.Microsoft }) return;
        if (MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount.Data != null)
        {
            var obj = JObject.Parse(MinecraftPluginData.MinecraftPluginSettingEntry.UsingMinecraftAccount.Data);
            var uuid = obj["Uuid"].ToString();
            MicrosoftSkinFetcher skinFetcher = new(uuid);
            var skin = await skinFetcher.GetSkinAsync();
        }
    }
}