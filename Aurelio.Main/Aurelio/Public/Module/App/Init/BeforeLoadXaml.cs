using System.Net;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App.Init.Config;
using Aurelio.Public.Module.Ui;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
        Update.Main();
        LangHelper.Current.ChangedCulture("");
        Setter.SetAccentColor(Color.Parse("#9373EE"));
        InitMl();
    }

    public static void InitMl()
    {
        HttpUtil.Initialize();
        DownloadMirrorManager.MaxThread = 128;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }
}