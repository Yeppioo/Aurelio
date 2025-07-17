using System.Diagnostics;
using System.IO;
using System.Linq;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.Service.Minecraft;
using Aurelio.Public.Module.Ui;
using Avalonia.Media;
using AurelioPage = Aurelio.Views.Main.Pages.Instance.SubPages.SettingPages.AurelioPage;

namespace Aurelio.Public.Module.App.Init;

public abstract class AfterUiLoaded
{
    public static void Main()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            Process.GetCurrentProcess().MainModule.FileName);
        BindKeys.Main(Aurelio.App.UiRoot!);
        _ = HandleMinecraftInstances.Load(Data.SettingEntry.MinecraftFolderEntries.Select(x => x.Path).ToArray());
        Setter.SetAccentColor(Color.Parse("#1BD76A"));
        _ = TranslateToken.RefreshToken();
        Setter.ToggleTheme(Data.SettingEntry.Theme);
        LoopGC.BeginLoop();
        if (Data.SettingEntry.AutoCheckUpdate && Data.Instance.Version != "vDebug")
            _ = AurelioPage.ShowUpdateDialogIfNeed();
    }
}