using System.Diagnostics;
using System.IO;
using System.Linq;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.Ui;
using Avalonia.Media;
using HandleInstances = Aurelio.Public.Module.Service.Minecraft.HandleInstances;

namespace Aurelio.Public.Module.App.Init;

public abstract class AfterUiLoaded
{
    public static void Main()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            Process.GetCurrentProcess().MainModule.FileName);
        BindKeys.Main();
        _ = HandleInstances.Load(Data.SettingEntry.MinecraftFolderEntries.Select(x => x.Path).ToArray());
        Setter.SetAccentColor(Color.Parse("#1BD76A"));
        _ = TranslateToken.RefreshToken();
        LoopGC.BeginLoop();
    }
}