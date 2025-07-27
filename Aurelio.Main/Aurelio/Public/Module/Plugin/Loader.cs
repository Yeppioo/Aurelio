using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aurelio.Plugin.Base;

namespace Aurelio.Public.Module.Plugin;

public class Loader
{
    public static Assembly LoadPlugin(string path)
    {
        Console.WriteLine($"Loading commands from: {path}");
        var loadContext = new PluginLoadContext(path);
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
    }

    public static IEnumerable<IPlugin> CreateCommands(Assembly assembly)
    {
        var count = 0;

        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type)) continue;
            if (Activator.CreateInstance(type) is not IPlugin result) continue;
            count++;
            yield return result;
        }

        if (count == 0)
        {
            // No IPlugin implementations found - treat this assembly as a dependency library
            Console.WriteLine($"No IPlugin implementations found in {assembly.GetName().Name} - treating as dependency library");
        }
    }
}