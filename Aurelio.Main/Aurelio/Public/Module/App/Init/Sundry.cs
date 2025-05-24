using System;
using System.Runtime.InteropServices;
using Aurelio.Public.Enum;

namespace Aurelio.Public.Module.App.Init;

public static class Sundry
{
    public static void DetectPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Running on Windows");
            Const.Data.DesktopType = DesktopType.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("Running on Linux");
            Const.Data.DesktopType = DesktopType.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("Running on macOS");
            Const.Data.DesktopType = DesktopType.MacOs;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Console.WriteLine("Running on FreeBSD");
            Const.Data.DesktopType = DesktopType.FreeBSD;
        }
        else
        {
            Console.WriteLine("Running on an unknown platform");
            Const.Data.DesktopType = DesktopType.Unknown;
        }
    }
}