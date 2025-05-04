using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aurelio.Plugin.Base;
using Aurelio.Public.Const;
using Aurelio.Public.Module.IO;

namespace Aurelio.Plugin.Services;

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

        if (count != 0) yield break;
        var availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
        throw new ApplicationException(
            $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
            $"Available types: {availableTypes}");
    }
    
    public static void ScanPlugin()
    {
        var dlls = Getter.GetAllFilesByExtension(ConfigPath.PluginFolderPath, "*.dll");
        dlls.ForEach(dll =>
        {
            try
            {
                var pluginAssembly = LoadPlugin(dll);
                var plugins = CreateCommands(pluginAssembly);
                plugins.ToList().ForEach(plugin =>
                {
                    try
                    {
                        plugin.Execute();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
            catch
            {
            }
        });
    }
}