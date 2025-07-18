using System.Net;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App.Init.Config;
using Aurelio.Public.Module.Ui;
using Avalonia.Media;
using MinecraftLaunch;
using MinecraftLaunch.Utilities;

namespace Aurelio.Public.Module.App.Init;

public abstract class BeforeLoadXaml
{
    public static void Main()
    {
        Sundry.DetectPlatform();
        Create.Main();
        Reader.Main();
        InitLanguage(Data.SettingEntry.Language.Code);
        Update.Main();
        InitMl();
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