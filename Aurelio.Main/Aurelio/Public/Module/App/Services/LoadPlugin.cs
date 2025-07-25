using System.Collections.Generic;
using System.Linq;
using Aurelio.Plugin.Base;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Plugin;
using SharpCompress;

namespace Aurelio.Public.Module.App.Services;

public class LoadPlugin
{
    public static void ScanPlugin()
    {
        // Clear existing plugins to avoid duplicates
        Data.LoadedPlugins.Clear();

        // Load all plugins first
        LoadAllPlugins();

        // Then validate dependencies
        ValidatePluginDependencies();

        Logger.Info($"Successfully loaded {Data.LoadedPlugins.Count} plugins");
    }

    private static void LoadAllPlugins()
    {
        var dlls = Getter.GetAllFilesByExtension(ConfigPath.PluginFolderPath, "*.dll");

        if (dlls.Count == 0)
        {
            Logger.Info("No plugin DLLs found in plugin folder");
            return;
        }

        Logger.Info($"Found {dlls.Count} plugin DLLs to load");

        var loadedCount = 0;
        var failedCount = 0;

        foreach (var dll in dlls)
        {
            try
            {
                var pluginAssembly = Loader.LoadPlugin(dll);
                var plugins = Loader.CreateCommands(pluginAssembly);

                var pluginList = plugins.ToList(); // Materialize to avoid multiple enumeration
                foreach (var plugin in pluginList)
                {
                    Data.LoadedPlugins.Add(plugin);
                    loadedCount++;
                    Logger.Info($"Loaded plugin: {plugin.Id} v{plugin.Version} by {plugin.Author}");
                }
            }
            catch (Exception e)
            {
                failedCount++;
                Logger.Error($"Failed to load plugin from {dll}: {e.Message}");
                Logger.Error(e);
            }
        }

        Logger.Info($"Plugin loading completed: {loadedCount} loaded, {failedCount} failed");
    }

    private static void ValidatePluginDependencies()
    {
        if (Data.LoadedPlugins.Count == 0)
        {
            Logger.Info("No plugins to validate dependencies for");
            return;
        }

        // Create a dictionary for O(1) plugin lookups with version info
        var availablePlugins = Data.LoadedPlugins.ToDictionary(p => p.Id, p => p);
        var pluginsToRemove = new List<IPlugin>();

        foreach (var plugin in Data.LoadedPlugins.OrderBy(p => p.Id))
        {
            // Check if plugin has Required property using reflection (since it's not in interface)
            var requireProperty = plugin.GetType().GetProperty("Require");
            if (requireProperty?.GetValue(plugin) is not IEnumerable<RequirePluginEntry> requirements)
                continue;

            var requirementsList = requirements.ToList();
            if (requirementsList.Count == 0)
                continue;

            var failedDependencies = new List<string>();

            foreach (var requirement in requirementsList)
            {
                if (string.IsNullOrEmpty(requirement.Id))
                    continue;

                if (!availablePlugins.TryGetValue(requirement.Id, out var dependencyPlugin))
                {
                    failedDependencies.Add($"{requirement.Id} (not found)");
                    continue;
                }

                // Validate version requirements
                if (!ValidateVersionRequirement(dependencyPlugin.Version, requirement))
                {
                    var versionInfo = GetVersionRequirementDescription(requirement);
                    failedDependencies.Add($"{requirement.Id} (version mismatch: found v{dependencyPlugin.Version}, {versionInfo})");
                }
            }

            if (failedDependencies.Count > 0)
            {
                Logger.Error($"Plugin '{plugin.Id}' has unmet dependencies: {string.Join(", ", failedDependencies)}");
                pluginsToRemove.Add(plugin);
            }
        }

        // Remove plugins with missing dependencies
        foreach (var plugin in pluginsToRemove)
        {
            Data.LoadedPlugins.Remove(plugin);
            Logger.Warning($"Removed plugin '{plugin.Id}' due to unmet dependencies");
        }

        Logger.Info(pluginsToRemove.Count > 0
            ? $"Dependency validation completed: {pluginsToRemove.Count} plugins removed due to unmet dependencies"
            : "All plugin dependencies validated successfully");
    }

    private static bool ValidateVersionRequirement(Version actualVersion, RequirePluginEntry requirement)
    {
        if (requirement.VersionRange == null || requirement.VersionRange.Length == 0)
            return true; // No version requirement

        try
        {
            var requiredVersion = requirement.VersionRange[0];

            return requirement.RequireMethod switch
            {
                RequireMethod.Equal => actualVersion.CompareTo(requiredVersion) == 0,
                RequireMethod.GreaterThan => actualVersion.CompareTo(requiredVersion) > 0,
                RequireMethod.LessThan => actualVersion.CompareTo(requiredVersion) < 0,
                RequireMethod.GreaterThanOrEqual => actualVersion.CompareTo(requiredVersion) >= 0,
                RequireMethod.LessThanOrEqual => actualVersion.CompareTo(requiredVersion) <= 0,
                _ => true
            };
        }
        catch (Exception e)
        {
            Logger.Warning($"Failed to validate version requirement: {e.Message}");
            return true; // If version validation fails, assume it's valid
        }
    }

    private static string GetVersionRequirementDescription(RequirePluginEntry requirement)
    {
        if (requirement.VersionRange == null || requirement.VersionRange.Length == 0)
            return "any version";

        return requirement.RequireMethod switch
        {
            RequireMethod.Equal => $"requires exactly v{requirement.VersionRange[0]}",
            RequireMethod.GreaterThan => $"requires > v{requirement.VersionRange[0]}",
            RequireMethod.LessThan => $"requires < v{requirement.VersionRange[0]}",
            RequireMethod.GreaterThanOrEqual => $"requires >= v{requirement.VersionRange[0]}",
            RequireMethod.LessThanOrEqual => $"requires <= v{requirement.VersionRange[0]}",
            _ => "unknown requirement"
        };
    }

    public static void ExecuteBeforeReadSettings()
    {
        Data.LoadedPlugins.ForEach(p => p.ExecuteBeforeReadSettings());
    }
    
    public static void ExecuteBeforeUiLoaded()
    {
        Data.LoadedPlugins.ForEach(p => p.ExecuteBeforeUiLoaded());
    }
    
    public static void ExecuteAfterUiLoaded()
    {
        Data.LoadedPlugins.ForEach(p => p.ExecuteAfterUiLoaded());
    }
}