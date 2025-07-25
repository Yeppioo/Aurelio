using System.Net;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App.Init.Config;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.Ui;
using Avalonia.Media;
using MinecraftLaunch;
using MinecraftLaunch.Utilities;
using Update = Aurelio.Public.Module.App.Init.Config.Update;

namespace Aurelio.Public.Module.App.Init;

public abstract class BeforeLoadXaml
{
    public static void Main()
    {
        Sundry.DetectPlatform();
        Create.Main();
        LoadPlugin.ScanPlugin();
        LoadPlugin.ExecuteBeforeReadSettings();
        Reader.Main();
        InitLanguage(Data.SettingEntry.Language.Code);
        Update.Main();
        InitMl();
        LoadPlugin.ExecuteBeforeUiLoaded();
    }

    public static void InitMl()
    {
        HttpUtil.Initialize();
        DownloadMirrorManager.MaxThread = 128;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }

    public static void InitLanguage(string code)
    {
        LangHelper.Current.ChangedCulture(code == "zh-CN" ? "" : code);
    }
}