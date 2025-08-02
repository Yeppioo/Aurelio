using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Aurelio.Plugin.Base;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Module.IO;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Plugin;

namespace Aurelio.Public.Module.App.Services;

public class LoadPlugin
{
    public static void ScanPlugin()
    {
        // Clear existing plugins to avoid duplicates
        Data.LoadedPlugins.Clear();

        // Clean up temporary plugin folder from previous runs
        CleanupTempFolder();

        // Ensure temporary plugin folder exists
        Setter.TryCreateFolder(ConfigPath.PluginTempFolderPath);

        // Load all plugins first
        LoadAllPlugins();

        // Then validate dependencies
        ValidatePluginDependencies();

        Logger.Info($"Successfully loaded {Data.LoadedPlugins.Count} plugins");
    }

    private static void LoadAllPlugins()
    {
        var loadedCount = 0;
        var failedCount = 0;

        // Load regular plugins (.nupkg files)
        loadedCount += LoadNupkgPlugins(ref failedCount);

        // Load debug plugins (.dll files)
        loadedCount += LoadDebugPlugins(ref failedCount);

        Logger.Info($"Plugin loading completed: {loadedCount} loaded, {failedCount} failed");
    }

    private static int LoadNupkgPlugins(ref int failedCount)
    {
        // Support both .aupkg (new format) and .nupkg (legacy format) for backward compatibility
        var aupkgFiles = Getter.GetAllFilesByExtension(ConfigPath.PluginFolderPath, "*.aupkg");
        var nupkgFiles = Getter.GetAllFilesByExtension(ConfigPath.PluginFolderPath, "*.nupkg");

        // Auto-rename .nupkg files to .aupkg for consistency
        var renamedFiles = new List<string>();
        foreach (var nupkgFile in nupkgFiles)
        {
            try
            {
                var aupkgPath = Path.ChangeExtension(nupkgFile, ".aupkg");

                // Check if .aupkg version already exists to avoid conflicts
                if (!File.Exists(aupkgPath))
                {
                    File.Move(nupkgFile, aupkgPath);
                    renamedFiles.Add(aupkgPath);
                    Logger.Info($"Renamed {Path.GetFileName(nupkgFile)} to {Path.GetFileName(aupkgPath)}");
                }
                else
                {
                    // If .aupkg already exists, delete the .nupkg file to avoid duplicates
                    File.Delete(nupkgFile);
                    Logger.Info($"Deleted duplicate {Path.GetFileName(nupkgFile)} as {Path.GetFileName(aupkgPath)} already exists");
                }
            }
            catch (Exception e)
            {
                Logger.Warning($"Failed to rename {Path.GetFileName(nupkgFile)} to .aupkg: {e.Message}");
                // If rename fails, keep the original file for loading
                renamedFiles.Add(nupkgFile);
            }
        }

        var allPackageFiles = new List<string>();
        allPackageFiles.AddRange(aupkgFiles);
        allPackageFiles.AddRange(renamedFiles);

        if (allPackageFiles.Count == 0)
        {
            Logger.Info("No plugin packages found in plugin folder");
            return 0;
        }

        Logger.Info($"Found {allPackageFiles.Count} plugin packages to load (including {renamedFiles.Count} renamed from .nupkg)");

        var loadedCount = 0;

        foreach (var packageFile in allPackageFiles)
        {
            try
            {
                var packageLoadedCount = ExtractAndLoadPluginPackage(packageFile);
                loadedCount += packageLoadedCount;
            }
            catch (Exception e)
            {
                failedCount++;
                Logger.Error($"Failed to load plugin package {Path.GetFileNameWithoutExtension(packageFile)}: {e.Message}");
                Logger.Error(e);
            }
        }

        return loadedCount;
    }

    private static int LoadDebugPlugins(ref int failedCount)
    {
        if (!Directory.Exists(ConfigPath.PluginDebugFolderPath))
        {
            Logger.Info("Debug plugin folder does not exist, skipping debug plugin loading");
            return 0;
        }

        var dllFiles = Getter.GetAllFilesByExtension(ConfigPath.PluginDebugFolderPath, "*.dll");

        if (dllFiles.Count == 0)
        {
            Logger.Info("No debug plugin files (.dll) found in debug plugin folder");
            return 0;
        }

        Logger.Info($"Found {dllFiles.Count} debug plugin files to load");

        try
        {
            return LoadPluginsFromDirectory(ConfigPath.PluginDebugFolderPath);
        }
        catch (Exception e)
        {
            failedCount++;
            Logger.Error($"Failed to load debug plugins: {e.Message}");
            Logger.Error(e);
            return 0;
        }
    }

    private static void ValidatePluginDependencies()
    {
        if (Data.LoadedPlugins.Count == 0)
        {
            Logger.Info("No plugins to validate dependencies for");
            return;
        }

        // Create a dictionary for O(1) plugin lookups with version info
        var availablePlugins = Data.LoadedPlugins.ToDictionary(p => p.Plugin.Id, p => p.Plugin);
        var pluginsToRemove = new List<LoadedPluginEntry>();

        foreach (var loadedPluginEntry in Data.LoadedPlugins.OrderBy(p => p.Plugin.Id))
        {
            var plugin = loadedPluginEntry.Plugin;

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
                pluginsToRemove.Add(loadedPluginEntry);
            }
        }

        // Remove plugins with missing dependencies
        foreach (var pluginEntry in pluginsToRemove)
        {
            Data.LoadedPlugins.Remove(pluginEntry);
            Logger.Warning($"Removed plugin '{pluginEntry.Plugin.Id}' due to unmet dependencies");
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

    private static void CleanupTempFolder()
    {
        try
        {
            if (Directory.Exists(ConfigPath.PluginTempFolderPath))
            {
                Setter.ClearFolder(ConfigPath.PluginTempFolderPath);
                Logger.Info("Cleaned up temporary plugin folder");
            }
        }
        catch (Exception e)
        {
            Logger.Warning($"Failed to cleanup temporary plugin folder: {e.Message}");
        }
    }

    private static int ExtractAndLoadPluginPackage(string nupkgPath)
    {
        var packageName = Path.GetFileNameWithoutExtension(nupkgPath);
        var extractPath = Path.Combine(ConfigPath.PluginTempFolderPath, packageName);

        Logger.Info($"Processing plugin package: {packageName}");

        // Create directory for this plugin package
        Setter.TryCreateFolder(extractPath);

        // Extract the plugin package
        ExtractPluginPackage(nupkgPath, extractPath);

        // Load plugins from the extracted directory
        return LoadPluginsFromDirectory(extractPath);
    }

    private static void ExtractPluginPackage(string nupkgPath, string extractPath)
    {
        using var archive = ZipFile.OpenRead(nupkgPath);

        foreach (var entry in archive.Entries)
        {
            // Only extract files from the bin/ directory
            if (!entry.FullName.StartsWith("bin/", StringComparison.OrdinalIgnoreCase) ||
                entry.FullName.EndsWith("/"))
                continue;

            // Remove "bin/" prefix from the path
            var relativePath = entry.FullName.Substring(4);
            var destinationPath = Path.Combine(extractPath, relativePath);

            // Ensure the directory exists
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Setter.TryCreateFolder(destinationDir);
            }

            // Extract the file
            entry.ExtractToFile(destinationPath, overwrite: true);
        }

        Logger.Info($"Extracted plugin package to: {extractPath}");
    }

    private static int LoadPluginsFromDirectory(string directory)
    {
        var dlls = Getter.GetAllFilesByExtension(directory, "*.dll");
        var loadedCount = 0;

        foreach (var dll in dlls)
        {
            try
            {
                var pluginAssembly = Loader.LoadPlugin(dll);
                var plugins = Loader.CreateCommands(pluginAssembly);

                var pluginList = plugins.ToList(); // Materialize to avoid multiple enumeration
                if (pluginList.Count == 0)
                {
                    // No plugins found - this DLL is treated as a dependency library
                    Logger.Info($"No plugins found in {Path.GetFileNameWithoutExtension(dll)} - treating as dependency library");
                }
                else
                {
                    foreach (var plugin in pluginList)
                    {
                        var loadedPluginEntry = new LoadedPluginEntry
                        {
                            Plugin = plugin
                        };

                        Data.LoadedPlugins.Add(loadedPluginEntry);
                        loadedCount++;
                        Logger.Info($"Loaded plugin: {plugin.Id} v{plugin.Version} by {plugin.Author}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load plugin from {dll}: {e.Message}");
                Logger.Error(e);
                // Don't increment failed count here as it's handled at package level
            }
        }

        return loadedCount;
    }

    public static void ExecutePlugin()
    {
        foreach (var loadedPluginEntry in Data.LoadedPlugins)
        {
            loadedPluginEntry.Plugin.Execute();
        }
    }
}