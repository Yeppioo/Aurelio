using System.Diagnostics;
using System.IO;
using Aurelio.Public.Module.Plugin.Events;
using Newtonsoft.Json;

namespace Aurelio.Public.Module.App;

public class AppMethod
{
    private static readonly Debouncer _debouncer = new(() =>
    {
        if (Data.SettingEntry is null) return;
        AppEvents.OnSaveSettings();
        File.WriteAllText(ConfigPath.SettingDataPath,
            JsonConvert.SerializeObject(Data.SettingEntry, Formatting.Indented));
    }, 300);

    public static void SaveSetting()
    {
        _debouncer.Trigger();
    }

    public static async void RestartApp(bool isAdmin = false)
    {
        if (!await AppEvents.OnAppExiting()) return;
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Process.GetCurrentProcess().MainModule.FileName
        };
        if (isAdmin) startInfo.Verb = "runas";
        Process.Start(startInfo);
        Environment.Exit(0);
    }

    public static async void TryExitApp()
    {
        if (!await AppEvents.OnAppExiting()) return;
        Environment.Exit(0);
    }
}