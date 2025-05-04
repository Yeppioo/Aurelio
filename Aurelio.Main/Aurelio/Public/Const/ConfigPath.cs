using System;
using System.IO;

namespace Aurelio.Public.Const;

public class ConfigPath
{
    public static string RootFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Yep.Aurelio");
    public static string PluginFolderPath => Path.Combine(RootFolderPath, "Aurelio.Plugin");
}