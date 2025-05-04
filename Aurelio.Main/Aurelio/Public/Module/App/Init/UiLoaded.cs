using Aurelio.Plugin.Services;

namespace Aurelio.Public.Module.App.Init;

public class UiLoaded
{
    public static void Main()
    {
        Loader.ScanPlugin();
    }
}