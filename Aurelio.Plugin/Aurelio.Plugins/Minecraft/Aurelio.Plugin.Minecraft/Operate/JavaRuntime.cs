using System.Diagnostics;
using Aurelio.Plugin.Minecraft.Classes.Minecraft;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Utilities;

namespace Aurelio.Plugin.Minecraft.Operate;

public class JavaRuntime
{
    public static async void AddByAutoScan()
    {
        try
        {
            var repeatJavaCount = 0;
            var successAddCount = 0;
            
            var i = JavaUtil.EnumerableJavaAsync();
            List<JavaEntry> javaList = [];
            await foreach (var item in i.ConfigureAwait(false))
                javaList.Add(item);

            var convertedJavaList = javaList.Select(RecordJavaRuntime.MlToAurelio).ToList();

            convertedJavaList.ForEach(java =>
            {
                if (!MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Contains(java))
                {
                    MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Add(java);
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
            if (MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Contains(
                    RecordJavaRuntime.MlToAurelio(javaInfo!)))
                Notice(MainLang.TheItemAlreadyExist, NotificationType.Error);
            else
                MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Add(
                    RecordJavaRuntime.MlToAurelio(javaInfo!));
        }

        VerifyList();
        AppMethod.SaveSetting();
    }

    public static void VerifyList()
    {
        if (!MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Contains(new RecordJavaRuntime
                { JavaVersion = "auto", JavaPath = MainLang.AutoChooseRightJavaRuntime }))
            MinecraftPluginData.MinecraftPluginSettingEntry.JavaRuntimes.Insert(0, new RecordJavaRuntime
                { JavaVersion = "auto", JavaPath = MainLang.AutoChooseRightJavaRuntime });
    }
}