using System.Diagnostics;
using System.IO;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Public.Module.Ui;
using AurelioPage = Aurelio.Views.Main.Pages.SubPages.SettingPages.AurelioPage;

namespace Aurelio.Public.Module.App.Init;

public abstract class AfterUiLoaded
{
    public static void Main()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            Process.GetCurrentProcess().MainModule.FileName);
        BindKeys.Main(Aurelio.App.UiRoot!);
        Setter.SetAccentColor(Data.SettingEntry.ThemeColor);
        Application.Current.Resources["BackGroundOpacity"] = Data.SettingEntry.BackGround == Setting.BackGround.Default ? 1.0 : 0.5;
        _ = TranslateToken.RefreshToken();
        Setter.ToggleTheme(Data.SettingEntry.Theme);
        LoopGC.BeginLoop();
        if (Data.SettingEntry.AutoCheckUpdate && Data.Instance.Version != "vDebug")
            _ = AurelioPage.ShowUpdateDialogIfNeed();
        AppEvents.OnAfterUiLoaded();
    }
}