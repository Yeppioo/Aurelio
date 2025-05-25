using System;
using System.Diagnostics;
using System.IO;
using Aurelio.Public.Const;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App;

public class AppMethod
{
    private static readonly Debouncer _debouncer = new(() =>
    {
        if (Data.SettingEntry is null) return;
        File.WriteAllText(ConfigPath.SettingDataPath,
            JsonConvert.SerializeObject(Data.SettingEntry, Formatting.Indented));
    }, 100);

    public static void SaveSetting()
    {
        _debouncer.Trigger();
    }

    public static void RestartApp(bool isAdmin = false)
    {
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Process.GetCurrentProcess().MainModule.FileName
        };
        if (isAdmin)
        {
            startInfo.Verb = "runas";
        }
        Process.Start(startInfo);
        Environment.Exit(0);
    }
}