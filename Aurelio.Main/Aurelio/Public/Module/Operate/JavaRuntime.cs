using System.Diagnostics;
using System.Linq;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Local;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Utilities;

namespace Aurelio.Public.Module.Operate;

public class JavaRuntime
{
    public static async void AddByAutoScan()
    {
        try
        {
            var repeatJavaCount = 0;
            var successAddCount = 0;
            var javaList = await JavaUtil.EnumerableJavaAsync().ToListAsync();
            var convertedJavaList = javaList.Select(RecordJavaRuntime.MlToAurelio).ToList();

            convertedJavaList.ForEach(java =>
            {
                if (!Data.SettingEntry.JavaRuntimes.Contains(java))
                {
                    Data.SettingEntry.JavaRuntimes.Add(java);
                    successAddCount++;
                }
                else
                {
                    repeatJavaCount++;
                }
            });

            Notice(
                $"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}",
                NotificationType.Success);
            VerifyList();
            AppMethod.SaveSetting();
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Notice(MainLang.OperateFailed, NotificationType.Error);
        }
    }

    public static async Task AddByUi(Control sender)
    {
        var list = await sender.PickFileAsync(
            new FilePickerOpenOptions { AllowMultiple = false, Title = MainLang.SelectJava });
        if (list.Count == 0 || string.IsNullOrWhiteSpace(list[0])) return;
        JavaEntry javaInfo = null;
        try
        {
            if (Data.DesktopType is DesktopType.Linux or DesktopType.MacOs)
                // 给Java赋予执行权限 (Linux和MacOs) 通过命令行 chmod +777
                await Task.Run(() =>
                {
                    var process = new Process();
                    process.StartInfo.FileName = "chmod";
                    process.StartInfo.Arguments = "+777 " + list[0];
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                });

            javaInfo = await JavaUtil.GetJavaInfoAsync(list[0]);
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Notice(MainLang.GetJavaInfoFail, NotificationType.Error);
        }

        if (javaInfo == null)
        {
            Notice(MainLang.GetJavaInfoFail, NotificationType.Error);
        }
        else
        {
            if (Data.SettingEntry.JavaRuntimes.Contains(RecordJavaRuntime.MlToAurelio(javaInfo!)))
                Notice(MainLang.TheItemAlreadyExist, NotificationType.Error);
            else
                Data.SettingEntry.JavaRuntimes.Add(RecordJavaRuntime.MlToAurelio(javaInfo!));
        }

        VerifyList();
        AppMethod.SaveSetting();
    }

    public static void VerifyList()
    {
        if (!Data.SettingEntry.JavaRuntimes.Contains(new RecordJavaRuntime
                { JavaVersion = "auto", JavaPath = MainLang.AutoChooseRightJavaRuntime }))
            Data.SettingEntry.JavaRuntimes.Insert(0, new RecordJavaRuntime
                { JavaVersion = "auto", JavaPath = MainLang.AutoChooseRightJavaRuntime });
    }
}