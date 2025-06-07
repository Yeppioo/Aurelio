using System.IO;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Const;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.App.Init.Services;
using Aurelio.Public.Module.Ui;
using Avalonia.Controls.Notifications;

namespace Aurelio.Public.Module.App.Init;

public abstract class AfterUiLoaded
{
    public static void Main()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        BindKeys.Main();
    }
}