using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Aurelio.Plugin.Base;

// 简单的插件加载测试
public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            Console.WriteLine($"Loading assembly: {assemblyName.Name} from {assemblyPath}");
            return LoadFromAssemblyPath(assemblyPath);
        }
        Console.WriteLine($"Could not resolve assembly: {assemblyName.Name}");
        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}

class Program
{
    static void Main()
    {
        var pluginPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yeppioo.Aurelio", "Yeppioo.Plugins", "Aurelio.Plugin.Minecraft.dll");

        Console.WriteLine($"Testing plugin load from: {pluginPath}");
        Console.WriteLine($"Plugin exists: {File.Exists(pluginPath)}");

        if (!File.Exists(pluginPath))
        {
            Console.WriteLine("Plugin file not found!");
            return;
        }

        try
        {
            Console.WriteLine("Creating PluginLoadContext...");
            var loadContext = new PluginLoadContext(pluginPath);
            
            Console.WriteLine("Loading assembly...");
            var assembly = loadContext.LoadFromAssemblyName(new AssemblyName("Aurelio.Plugin.Minecraft"));
            
            Console.WriteLine($"Assembly loaded: {assembly.FullName}");
            Console.WriteLine($"Assembly location: {assembly.Location}");
            
            Console.WriteLine("Getting types...");
            var types = assembly.GetTypes();
            Console.WriteLine($"Found {types.Length} types");
            
            foreach (var type in types)
            {
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    Console.WriteLine($"Found plugin type: {type.FullName}");
                    
                    try
                    {
                        Console.WriteLine("Creating plugin instance...");
                        var plugin = Activator.CreateInstance(type) as IPlugin;
                        
                        if (plugin != null)
                        {
                            Console.WriteLine($"Plugin created successfully!");
                            Console.WriteLine($"  ID: {plugin.Id}");
                            Console.WriteLine($"  Name: {plugin.Name}");
                            Console.WriteLine($"  Author: {plugin.Author}");
                            Console.WriteLine($"  Version: {plugin.Version}");
                            Console.WriteLine($"  Description: {plugin.Description}");
                        }
                        else
                        {
                            Console.WriteLine("Failed to create plugin instance (null)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating plugin instance: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading plugin: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
