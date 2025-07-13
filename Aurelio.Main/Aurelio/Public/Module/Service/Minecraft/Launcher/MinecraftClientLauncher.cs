using System.Threading;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Authentication;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Launch;
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
            Logger.Error(e);
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
        task.ButtonText = MainLang.Cancel;
        task.IsButtonEnable = true;
        task.ButtonAction = () =>
        {
            task.CancelWaitFinish();
            cts.Cancel();
        };
        task.TaskState = TaskState.Running;
        new TaskEntry(MainLang.CheckLaunchArg).AddIn(task);
        new TaskEntry(MainLang.RefreshAccountToken).AddIn(task);
        new TaskEntry(MainLang.BuildLaunchConfig).AddIn(task);
        new TaskEntry(MainLang.LaunchMinecraftProcess).AddIn(task);


        task.NextSubTask();
        task.NextSubTask();

        _ = OpenTaskDrawer();

        Account? account;
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
                    if (task.IsCancelRequest)
                    {
                        task.CancelFinish();
                        return;
                    }

                    ShowShortException(MainLang.LoginFail, ex);
                    task.FinishWithError();
                    return;
                }

                break;
            case Setting.AccountType.ThirdParty:
                account = JsonConvert.DeserializeObject<YggdrasilAccount>(Data.SettingEntry.UsingMinecraftAccount
                    .Data!);
                break;
            default:
                throw new ArgumentOutOfRangeException();
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

        task.NextSubTask();

        var config = new LaunchConfig
        {
            Account = account,
            JavaPath = RecordJavaRuntime.AurelioToMl(setting.JavaRuntime),
            MaxMemorySize = Convert.ToInt32(setting.MemoryLimit),
            MinMemorySize = 512,
            IsEnableIndependency = setting.EnableIndependentMinecraft,
            JvmArguments = [],
            LauncherName = "Aurelio"
        };

        if (!string.IsNullOrWhiteSpace(setting.AutoJoinServerAddress))
        {
            var serverInfo = setting.AutoJoinServerAddress.Split(':');
            config.ServerInfo = new ServerInfo
            {
                Address = serverInfo[0],
                Port = Convert.ToInt32(serverInfo[1])
            };
        }

        task.NextSubTask();
        MinecraftRunner runner = new(config, new MinecraftParser(entry.ParentMinecraftFolder.Path));
        try
        {
            await Task.Run(async () =>
            {
                try
                {
                    var process = await runner.RunAsync(entry.Id, token);
                    var copyArguments = string.Join(" ", process.ArgumentList);
                    process.Exited += async (_, arg) =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            // if (Data.SettingEntry.LauncherVisibility !=
                            //     Setting.LauncherVisibility.AfterLaunchMakeLauncherMinimize)
                            // {
                            //     if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window1)
                            //     {
                            //         window1.Show();
                            //         window1.WindowState = WindowState.Normal;
                            //         window1.Activate();
                            //     }
                            // }

                            Notice($"{MainLang.GameExited} - {entry.Id}", NotificationType.Warning);

                            task.FinishWithSuccess();

                            // await Task.Delay(2000);
                            // if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window2)
                            // {
                            //     window2.Activate();
                            //     window2.Focus();
                            // }
                        });
                    };

                    process.OutputLogReceived += (_, arg) =>
                    {
                        // var regex = new Regex(@"^\[[^\]]*\]\s*\[([^\]]*?)(\]|$)(\s*.*)");
                        // var match = regex.Match(arg.Data.Source);
                        // var regStr = match.Groups[1].Value + match.Groups[3].Value;
                        // Dispatcher.UIThread.Invoke(
                        //     () =>
                        //     {
                        //         window.Append(arg.Data.Log, arg.Data.Time, (LogType)arg.Data.LogLevel,
                        //             string.IsNullOrWhiteSpace(regStr) ? arg.Data.Source : regStr);
                        //     },
                        //     DispatcherPriority.ApplicationIdle);
                        Logger.Info(arg.Data.ToString());
                    };

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.NextSubTask();
                        Notice($"{MainLang.LaunchFinish} - {entry.Id}", NotificationType.Success);
                        task.OperateButtons.Add(new OperateButtonEntry(MainLang.DisplayLaunchArguments,
                            async void () =>
                            {
                                var dialog = await ShowDialogAsync(MainLang.LaunchArguments,
                                    string.Join(" \n", process.ArgumentList), b_cancel: MainLang.Ok,
                                    b_primary: MainLang.Copy);
                                if (dialog != ContentDialogResult.Primary) return;
                                var clipboard = Aurelio.App.TopLevel.Clipboard;
                                await clipboard.SetTextAsync(copyArguments);
                                Notice(MainLang.AlreadyCopyToClipBoard, NotificationType.Success);
                            }));
                        task.OperateButtons.Add(new OperateButtonEntry(MainLang.KillProcess, void () =>
                        {
                            try
                            {
                                process.Process.Kill(true);
                                task.FinishWithSuccess();
                            }
                            catch
                            {
                                // ignored
                            }
                        }));
                        task.OperateButtons.Add(new OperateButtonEntry("显示Minecraft日志", () =>
                        {
                            // window.Show();
                            // window.Activate();
                        }));
                    });
                    _ = Task.Run(() =>
                    {
                        task.ButtonText = MainLang.KillProcess;
                        task.ButtonAction = () =>
                        {
                            try
                            {
                                process.Process.Kill(true);
                                task.FinishWithSuccess();
                            }
                            catch
                            {
                                // ignored
                            }
                        };

                        // await Task.Delay(8000);
                        // Dispatcher.UIThread.Invoke(() =>
                        // {
                        //     switch (Data.SettingEntry.LauncherVisibility)
                        //     {
                        //         case Setting.LauncherVisibility.AfterLaunchExitLauncher:
                        //             Environment.Exit(0);
                        //             break;
                        //         case Setting.LauncherVisibility.AfterLaunchMakeLauncherMinimize:
                        //         case Setting.LauncherVisibility.AfterLaunchMinimizeAndShowWhenGameExit:
                        //             if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window2)
                        //             {
                        //                 window2.WindowState = WindowState.Minimized;
                        //             }
                        //
                        //             break;
                        //         case Setting.LauncherVisibility.AfterLaunchHideAndShowWhenGameExit:
                        //             if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window1)
                        //             {
                        //                 window1.Hide();
                        //             }
                        //
                        //             break;
                        //         case Setting.LauncherVisibility.AfterLaunchKeepLauncherVisible:
                        //         default:
                        //             break;
                        //     }
                        // });
                    }, token);
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowShortException(MainLang.LaunchFail, ex);
                        task.FinishWithError();
                    });
                }
            }, token);
        }
        catch (OperationCanceledException)
        {
            Notice($"{MainLang.Canceled}: {MainLang.Launch} - {entry.Id}", NotificationType.Warning);
            task.CancelFinish();
        }
    }
}