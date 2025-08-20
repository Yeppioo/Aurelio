using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.ViewModels;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;

namespace Aurelio.Views.Overlay;

public partial class AggregateSearchDialog : PageMixModelBase
{
    private string _aggregateSearchFilter = "";
    public ObservableCollection<AggregateSearchEntry> FilteredAggregateSearchEntries { get; } = [];

    // File system search properties
    [Reactive]
    public StreamGeometry SearchIconData { get; set; } = StreamGeometry.Parse(
        "M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z");

    [Reactive] public bool IsFileSystemMode { get; set; }
    [Reactive] public string CurrentFileSystemPath { get; set; } = string.Empty;

    // Type filter mode properties
    [Reactive] public bool IsTypeFilterMode { get; set; }
    [Reactive] public string CurrentTypeFilter { get; set; } = string.Empty;
    [Reactive] public string CurrentKeywordFilter { get; set; } = string.Empty;

    public string AggregateSearchFilter
    {
        get => _aggregateSearchFilter;
        set
        {
            SetField(ref _aggregateSearchFilter, value);
            Filter();
        }
    }

    private void Filter()
    {
        try
        {
            FilteredAggregateSearchEntries.Clear();

            // Check if we're in file system search mode
            if (AggregateSearchFilter.StartsWith("!!"))
            {
                IsFileSystemMode = true;
                IsTypeFilterMode = false;
                var pathPart = AggregateSearchFilter.Substring(2);
                CurrentFileSystemPath = pathPart;

                // Update search icon based on whether we have a valid path
                UpdateSearchIcon();

                // Get file system entries
                var fileSystemEntries = GetFileSystemEntries(pathPart);
                FilteredAggregateSearchEntries.AddRange(fileSystemEntries);
            }
            // Check if we're in type filter mode
            else if (AggregateSearchFilter.StartsWith("#"))
            {
                IsFileSystemMode = false;
                IsTypeFilterMode = true;
                CurrentFileSystemPath = string.Empty;
                SearchIconData = StreamGeometry.Parse(
                    "M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z");

                var filterPart = AggregateSearchFilter.Substring(1); // Remove #
                ParseTypeFilter(filterPart);

                // Get type filtered entries
                var typeFilteredEntries = GetTypeFilteredEntries();
                FilteredAggregateSearchEntries.AddRange(typeFilteredEntries);
            }
            else
            {
                IsFileSystemMode = false;
                IsTypeFilterMode = false;
                CurrentFileSystemPath = string.Empty;
                SearchIconData = StreamGeometry.Parse(
                    "M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z");

                // Normal search mode
                FilteredAggregateSearchEntries.AddRange(Data.AggregateSearchEntries.Where(item =>
                        item.Title.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase) ||
                        item.Summary.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.Order).ThenBy(x => x.Title));
            }
        }
        catch
        {
            // ignored
        }
    }

    public AggregateSearchDialog()
    {
        InitializeComponent();
        Init();
    }

    private void Init()
    {
        DataContext = this;
        Data.AggregateSearchEntries.CollectionChanged += (_, _) => Filter();
        Width = 870;
        Height = 550;
        PointerMoved += (_, _) => { AggregateSearchBox.Focus(); };
        ComboBox.SelectionChanged += (_, _) => { Filter(); };
        AggregateSearchListBox.SelectionChanged += (_, _) =>
        {
            if (AggregateSearchListBox.SelectedItem is not AggregateSearchEntry entry) return;

            // Handle type filter selection
            if (IsTypeFilterMode && entry.Type is string && entry.Type.Equals("TypeFilter"))
            {
                HandleTypeFilterSelection(entry);
                return;
            }

            if (entry.Type is not AggregateSearchEntryType t)
            {
                var host = TopLevel.GetTopLevel(this);
                if (host is not DialogWindow window) return;
                AggregateSearch.Execute(entry, window.Owner!);
                window.Close();
                return;
            }

            if (t is AggregateSearchEntryType.SystemFile or AggregateSearchEntryType.SystemFileGoUp)
            {
                HandleFileSystemEntrySelection(entry);
                return;
            }

            if (t == AggregateSearchEntryType.AurelioTabPage)
            {
                var host = TopLevel.GetTopLevel(this);
                if (host is not DialogWindow window) return;
                AggregateSearch.Execute(entry, window.Owner!);
                OpenedNewPage?.Invoke(this, EventArgs.Empty);
                window.Close();
            }
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is not Key.Escape) return;
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        };
        Loaded += (_, _) => { AggregateSearchBox.Focus(); };

        // Add file system search event handlers
        AggregateSearchBox.KeyDown += AggregateSearchBox_KeyDown;
        AggregateSearchListBox.PointerPressed += AggregateSearchListBox_PointerPressed;

        Filter();
    }

    public event EventHandler? OpenedNewPage;

    #region File System Search Methods

    private void UpdateSearchIcon()
    {
        if (IsFileSystemMode && !string.IsNullOrEmpty(CurrentFileSystemPath) &&
            (Directory.Exists(CurrentFileSystemPath) || File.Exists(CurrentFileSystemPath)))
        {
            // Show right arrow icon when we have a valid path
            SearchIconData = Icons.ChevronRight;
        }
        else
        {
            // Show search icon
            SearchIconData = StreamGeometry.Parse(
                "M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z");
        }
    }

    private ObservableCollection<AggregateSearchEntry> GetFileSystemEntries(string pathPart)
    {
        var entries = new ObservableCollection<AggregateSearchEntry>();

        try
        {
            if (string.IsNullOrEmpty(pathPart))
            {
                // Show system roots
                entries.AddRange(GetSystemRoots());
            }
            else
            {
                // Handle partial path matching
                var normalizedPath = pathPart.Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);

                // If the path exists as a complete directory, show its contents
                if (Directory.Exists(normalizedPath))
                {
                    // Add "go up" entry if not at root level
                    var goUpEntry = GetGoUpEntry(normalizedPath);
                    if (goUpEntry != null)
                    {
                        entries.Add(goUpEntry);
                    }

                    entries.AddRange(GetDirectoryContents(normalizedPath));
                }
                else
                {
                    // Handle partial path matching
                    var parentPath = Path.GetDirectoryName(normalizedPath);
                    var partialName = Path.GetFileName(normalizedPath);

                    if (!string.IsNullOrEmpty(parentPath) && Directory.Exists(parentPath))
                    {
                        // Add "go up" entry if not at root level (always show when filtering)
                        var goUpEntry = GetGoUpEntry(parentPath);
                        if (goUpEntry != null)
                        {
                            entries.Add(goUpEntry);
                        }

                        // Show matching items from parent directory
                        entries.AddRange(GetPartialMatchingContents(parentPath, partialName));
                    }
                    else if (string.IsNullOrEmpty(parentPath))
                    {
                        // Handle root-level partial matching (e.g., "/h" should match "/home")
                        entries.AddRange(GetPartialMatchingRoots(partialName));
                    }
                }
            }
        }
        catch
        {
            // Handle permission errors gracefully
        }

        return entries;
    }

    private IEnumerable<AggregateSearchEntry> GetSystemRoots()
    {
        var roots = new List<AggregateSearchEntry>();

        try
        {
            if (Data.DesktopType == DesktopType.Windows)
            {
                // Windows: Show drive letters
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        roots.Add(new AggregateSearchEntry(drive.RootDirectory, true));
                    }
                }
            }
            else if (Data.DesktopType == DesktopType.Linux)
            {
                // Linux: Show root and common mount points
                var commonPaths = new[] { "/", "/home", "/usr", "/var", "/opt", "/tmp" };
                foreach (var path in commonPaths)
                {
                    if (Directory.Exists(path))
                    {
                        roots.Add(new AggregateSearchEntry(new DirectoryInfo(path), true));
                    }
                }
            }
            else if (Data.DesktopType == DesktopType.MacOs)
            {
                // macOS: Show root and common directories
                var commonPaths = new[] { "/", "/Users", "/Applications", "/System", "/Library" };
                foreach (var path in commonPaths)
                {
                    if (Directory.Exists(path))
                    {
                        roots.Add(new AggregateSearchEntry(new DirectoryInfo(path), true));
                    }
                }
            }
        }
        catch
        {
            // Handle errors gracefully
        }

        return roots;
    }

    private IEnumerable<AggregateSearchEntry> GetDirectoryContents(string directoryPath)
    {
        var contents = new List<AggregateSearchEntry>();

        try
        {
            var directory = new DirectoryInfo(directoryPath);

            // Add subdirectories first
            var subdirectories = directory.GetDirectories().Take(50); // Limit to 50 items for performance
            foreach (var subdir in subdirectories)
            {
                try
                {
                    contents.Add(new AggregateSearchEntry(subdir));
                }
                catch
                {
                    // Skip directories we can't access
                }
            }

            // Add files
            var files = directory.GetFiles().Take(50); // Limit to 50 items for performance
            foreach (var file in files)
            {
                try
                {
                    contents.Add(new AggregateSearchEntry(file));
                }
                catch
                {
                    // Skip files we can't access
                }
            }
        }
        catch
        {
            // Handle permission errors gracefully
        }

        return contents;
    }

    private IEnumerable<AggregateSearchEntry> GetPartialMatchingContents(string parentPath, string partialName)
    {
        var contents = new List<AggregateSearchEntry>();

        try
        {
            var directory = new DirectoryInfo(parentPath);

            // Add matching subdirectories
            var subdirectories = directory.GetDirectories()
                .Where(d => d.Name.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
                .Take(50);

            foreach (var subdir in subdirectories)
            {
                try
                {
                    contents.Add(new AggregateSearchEntry(subdir));
                }
                catch
                {
                    // Skip directories we can't access
                }
            }

            // Add matching files
            var files = directory.GetFiles()
                .Where(f => f.Name.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
                .Take(50);

            foreach (var file in files)
            {
                try
                {
                    contents.Add(new AggregateSearchEntry(file));
                }
                catch
                {
                    // Skip files we can't access
                }
            }
        }
        catch
        {
            // Handle permission errors gracefully
        }

        return contents;
    }

    private IEnumerable<AggregateSearchEntry> GetPartialMatchingRoots(string partialName)
    {
        var roots = new List<AggregateSearchEntry>();

        try
        {
            if (Data.DesktopType == DesktopType.Windows)
            {
                // Windows: Show matching drive letters
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.Name.StartsWith(partialName, StringComparison.OrdinalIgnoreCase));

                foreach (var drive in drives)
                {
                    roots.Add(new AggregateSearchEntry(drive.RootDirectory, true));
                }
            }
            else
            {
                // Linux/macOS: Show matching root directories
                var commonPaths = Data.DesktopType == DesktopType.Linux
                    ? new[] { "/", "/home", "/usr", "/var", "/opt", "/tmp" }
                    : new[] { "/", "/Users", "/Applications", "/System", "/Library" };

                foreach (var path in commonPaths)
                {
                    if (Directory.Exists(path))
                    {
                        var dirName = path == "/" ? "/" : Path.GetFileName(path);
                        if (dirName.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
                        {
                            roots.Add(new AggregateSearchEntry(new DirectoryInfo(path), true));
                        }
                    }
                }
            }
        }
        catch
        {
            // Handle errors gracefully
        }

        return roots;
    }

    private AggregateSearchEntry? GetGoUpEntry(string currentPath)
    {
        try
        {
            // Normalize the current path
            var normalizedPath = currentPath.TrimEnd(Path.DirectorySeparatorChar, '/');

            // Check if we're at drive root level (e.g., C:\ on Windows or / on Unix)
            if (Data.DesktopType == DesktopType.Windows)
            {
                var driveRoot = Path.GetPathRoot(normalizedPath);
                if (string.Equals(normalizedPath, driveRoot?.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                {
                    // At drive root (e.g., C:\) - go up to drive selection (empty path)
                    return new AggregateSearchEntry("");
                }
            }
            else if (Data.DesktopType == DesktopType.Linux || Data.DesktopType == DesktopType.MacOs)
            {
                if (normalizedPath == "/" || string.IsNullOrEmpty(normalizedPath))
                {
                    // At system root (/) - go up to drive selection (empty path)
                    return new AggregateSearchEntry("");
                }
            }

            // For subdirectories, get the actual parent directory
            var parentPath = Path.GetDirectoryName(normalizedPath);

            // If parentPath is null or empty, we're at a root level
            if (string.IsNullOrEmpty(parentPath))
            {
                return new AggregateSearchEntry("");
            }

            return new AggregateSearchEntry(parentPath);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Type Filter Methods

    private void ParseTypeFilter(string filterPart)
    {
        // Check if there's a space, which separates type filter from keyword filter
        var spaceIndex = filterPart.IndexOf(' ');
        if (spaceIndex >= 0)
        {
            CurrentTypeFilter = filterPart.Substring(0, spaceIndex);
            CurrentKeywordFilter = filterPart.Substring(spaceIndex + 1);
        }
        else
        {
            CurrentTypeFilter = filterPart;
            CurrentKeywordFilter = string.Empty;
        }
    }

    private ObservableCollection<AggregateSearchEntry> GetTypeFilteredEntries()
    {
        var entries = new ObservableCollection<AggregateSearchEntry>();

        try
        {
            // If no type filter is specified, show all available types
            if (string.IsNullOrEmpty(CurrentTypeFilter))
            {
                entries.AddRange(GetAvailableTypes());
            }
            else
            {
                // Filter entries by type and optionally by keyword
                var filteredEntries = Data.AggregateSearchEntries.Where(item =>
                {
                    // Check if the item's type contains the type filter (case insensitive)
                    var typeMatches = GetItemTypeString(item).Contains(CurrentTypeFilter, StringComparison.OrdinalIgnoreCase);

                    if (!typeMatches) return false;

                    // If there's a keyword filter, also check title and summary
                    if (!string.IsNullOrEmpty(CurrentKeywordFilter))
                    {
                        return item.Title.Contains(CurrentKeywordFilter, StringComparison.OrdinalIgnoreCase) ||
                               item.Summary.Contains(CurrentKeywordFilter, StringComparison.OrdinalIgnoreCase);
                    }

                    return true;
                });

                entries.AddRange(filteredEntries.OrderByDescending(x => x.Order).ThenBy(x => x.Title));
            }
        }
        catch
        {
            // Handle errors gracefully
        }

        return entries;
    }

    private IEnumerable<AggregateSearchEntry> GetAvailableTypes()
    {
        var typeEntries = new List<AggregateSearchEntry>();

        try
        {
            // Get all unique types from the aggregate search entries
            var uniqueTypes = Data.AggregateSearchEntries
                .Select(item => GetItemTypeString(item))
                .Where(type => !string.IsNullOrEmpty(type))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(type => type);

            foreach (var type in uniqueTypes)
            {
                var count = Data.AggregateSearchEntries.Count(item =>
                    GetItemTypeString(item).Equals(type, StringComparison.OrdinalIgnoreCase));

                typeEntries.Add(new AggregateSearchEntry
                {
                    Title = type,
                    Summary = $"{count} items of this type",
                    Icon = StreamGeometry.Parse("M256 80c0-17.7-14.3-32-32-32s-32 14.3-32 32l0 144L48 224c-17.7 0-32 14.3-32 32s14.3 32 32 32l144 0 0 144c0 17.7 14.3 32 32 32s32-14.3 32-32l0-144 144 0c17.7 0 32-14.3 32-32s-14.3-32-32-32l-144 0 0-144z"),
                    Type = "TypeFilter",
                    OriginObject = type,
                    Order = 100
                });
            }
        }
        catch
        {
            // Handle errors gracefully
        }

        return typeEntries;
    }

    private string GetItemTypeString(AggregateSearchEntry item)
    {
        if (item.Type == null) return "Unknown";

        // Handle enum types
        if (item.Type is AggregateSearchEntryType enumType)
        {
            return enumType.ToString();
        }

        // Handle string types
        if (item.Type is string stringType)
        {
            return stringType;
        }

        // Handle other object types
        return item.Type.GetType().Name;
    }

    private void HandleTypeFilterSelection(AggregateSearchEntry entry)
    {
        if (entry.OriginObject is not string selectedType) return;

        // Update the search filter to include the selected type
        var newFilter = "#" + selectedType + " ";

        Dispatcher.UIThread.Post(() =>
        {
            AggregateSearchFilter = newFilter;

            // Give focus to the search box and move cursor to end
            AggregateSearchBox.Focus();
            AggregateSearchBox.CaretIndex = AggregateSearchBox.Text?.Length ?? 0;
        });
    }

    #endregion

    #region Event Handlers

    private void HandleFileSystemEntrySelection(AggregateSearchEntry entry)
    {
        // Handle "go up" entry
        if (entry is { Type: AggregateSearchEntryType.SystemFileGoUp, OriginObject: string parentPath })
        {
            string newPath;

            if (string.IsNullOrEmpty(parentPath))
            {
                // Go back to drive selection
                newPath = "!!";
            }
            else
            {
                // Navigate to parent directory
                newPath = "!!" + parentPath;
                if (!newPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    newPath += Path.DirectorySeparatorChar;
                }
            }

            // Update the search filter which will trigger the Filter() method
            Dispatcher.UIThread.Post(() =>
            {
                AggregateSearchFilter = newPath;

                // Give focus to the search box and move cursor to end
                AggregateSearchBox.Focus();
                AggregateSearchBox.CaretIndex = AggregateSearchBox.Text?.Length ?? 0;
            });
            return;
        }

        // Handle regular file system entries
        if (entry.OriginObject is not FileSystemInfo fsInfo) return;

        if (fsInfo is DirectoryInfo)
        {
            // Directory: Navigate to this path
            var newPath = "!!" + fsInfo.FullName;
            if (!newPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                newPath += Path.DirectorySeparatorChar;
            }

            // Update the search filter which will trigger the Filter() method
            Dispatcher.UIThread.Post(() =>
            {
                AggregateSearchFilter = newPath;

                // Give focus to the search box and move cursor to end
                AggregateSearchBox.Focus();
                AggregateSearchBox.CaretIndex = AggregateSearchBox.Text?.Length ?? 0;
            });
        }
        else if (fsInfo is FileInfo)
        {
            // File: Open directly with system default application
            OpenFileSystemPath(fsInfo.FullName);

            // Close the dialog
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        }
    }

    private void AggregateSearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && IsFileSystemMode && !string.IsNullOrEmpty(CurrentFileSystemPath))
        {
            OpenFileSystemPath(CurrentFileSystemPath);

            // Close the dialog
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        }
    }

    private void SearchIcon_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsFileSystemMode && !string.IsNullOrEmpty(CurrentFileSystemPath))
        {
            OpenFileSystemPath(CurrentFileSystemPath);

            // Close the dialog
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        }
    }

    private void AggregateSearchListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed ||
            AggregateSearchListBox.SelectedItem is not AggregateSearchEntry entry ||
            entry.Type is not AggregateSearchEntryType.SystemFile) return;

        // Right-click: Open with system default
        if (entry.OriginObject is FileSystemInfo fsInfo)
        {
            OpenFileSystemPath(fsInfo.FullName);

            // Close the dialog
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        }
    }

    private async void OpenFileSystemPath(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var isNav = await FileNav.NavPage(path);
                if (isNav)
                {
                    return;
                }
                // Open file with default application
                var launcher = TopLevel.GetTopLevel(this)?.Launcher;
                if (launcher != null)
                {
                    _ = launcher.LaunchFileInfoAsync(new FileInfo(path));
                }
            }
            else if (Directory.Exists(path))
            {
                // Open folder with default file manager
                await OpenFolder(path);
            }
        }
        catch
        {
            // Handle errors gracefully
        }
    }

    #endregion
}