using System.Net;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App.Init.Config;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.Plugin.Events;
using Update = Aurelio.Public.Module.App.Init.Config.Update;

namespace Aurelio.Public.Module.App.Init;

public abstract class BeforeLoadXaml
{
    public static void Main()
    {
        Sundry.DetectPlatform();
        Create.Main();
        LoadPlugin.ScanPlugin();
        LoadPlugin.ExecutePlugin();
        InitEvents.OnBeforeReadSettings();
        Reader.Main();
        InitLanguage(Data.SettingEntry.Language.Code);
        Update.Main();
        InitEvents.OnBeforeUiLoaded();
    }

    public static void InitLanguage(string code)
    {
        GlobalLangHelper.Current.ChangedCulture(code == "zh-CN" ? "" : code);
    }
}