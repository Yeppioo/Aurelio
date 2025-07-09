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
            Data.DesktopType = DesktopType.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("Running on Linux");
            Data.DesktopType = DesktopType.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("Running on macOS");
            Data.DesktopType = DesktopType.MacOs;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Console.WriteLine("Running on FreeBSD");
            Data.DesktopType = DesktopType.FreeBSD;
        }
        else
        {
            Console.WriteLine("Running on an unknown platform");
            Data.DesktopType = DesktopType.Unknown;
        }
    }
}