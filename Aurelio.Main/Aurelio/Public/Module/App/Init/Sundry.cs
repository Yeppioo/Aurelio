using System.Runtime.InteropServices;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.IO;

namespace Aurelio.Public.Module.App.Init;

public static class Sundry
{
    public static void DetectPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Logger.Info("Running on Windows");
            Data.DesktopType = DesktopType.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Logger.Info("Running on Linux");
            Data.DesktopType = DesktopType.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Logger.Info("Running on macOS");
            Data.DesktopType = DesktopType.MacOs;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Logger.Info("Running on FreeBSD");
            Data.DesktopType = DesktopType.FreeBSD;
        }
        else
        {
            Logger.Info("Running on an unknown platform");
            Data.DesktopType = DesktopType.Unknown;
        }
    }
}