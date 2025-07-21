using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.IO.Http;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using SharpCompress.Archives;
using SharpCompress.Common;
using static Aurelio.Public.Module.Ui.Overlay;

namespace Aurelio.Views.Main.Pages.Viewers;

// Data Models
public class ArchiveEntryViewModel : INotifyPropertyChanged
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public long CompressedSize { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public DateTime LastModified { get; set; }
    public string IconPath { get; set; } = string.Empty;

    public string SizeFormatted => Size.CalcMemoryMensurableUnit();
    public string CompressedSizeFormatted => CompressedSize.CalcMemoryMensurableUnit();
    public string LastModifiedFormatted => LastModified.ToString("yyyy-MM-dd HH:mm:ss");

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 根据文件类型获取图标的矢量路径
    /// </summary>
    public static string GetIconPath(string fileName, bool isDirectory, bool isParentDirectory = false)
    {
        // 返回上级目录
        if (isParentDirectory)
        {
            return "F1 M 16.25 18.046875 L 3.75 18.046875 C 3.059654 18.046875 2.5 18.606529 2.5 19.296875 L 2.5 19.296875 C 2.5 19.987221 3.059654 20.546875 3.75 20.546875 L 16.25 20.546875 C 16.940346 20.546875 17.5 19.987221 17.5 19.296875 L 17.5 19.296875 C 17.5 18.606529 16.940346 18.046875 16.25 18.046875 Z M 4.6875 10.539742 L 7.5 10.539742 L 7.5 15.543289 C 7.5 16.234169 8.059692 16.794167 8.75 16.794167 L 11.25 16.794167 C 11.940384 16.794167 12.5 16.234169 12.5 15.543289 L 12.5 10.539742 L 15.3125 10.539742 C 15.686646 10.539742 16.025391 10.316811 16.173706 9.972878 C 16.321411 9.629021 16.251221 9.229584 15.994263 8.957138 L 10.681763 3.328133 C 10.327759 2.95311 9.672241 2.95311 9.318237 3.328133 L 4.005737 8.957138 C 3.748779 9.229584 3.678589 9.629021 3.826294 9.972878 C 3.974609 10.316811 4.313354 10.539742 4.6875 10.539742 Z"; // 占位符：返回上级目录图标
        }

        // 文件夹
        if (isDirectory)
        {
            return "M64 480H448c35.3 0 64-28.7 64-64V160c0-35.3-28.7-64-64-64H288c-10.1 0-19.6-4.7-25.6-12.8L243.2 57.6C231.1 41.5 212.1 32 192 32H64C28.7 32 0 60.7 0 96V416c0 35.3 28.7 64 64 64z"; // 占位符：文件夹图标
        }

        // 文件
        return "M64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-288-128 0c-17.7 0-32-14.3-32-32L224 0 64 0zM256 0l0 128 128 0L256 0zM112 256l160 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-160 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l160 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-160 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm0 64l160 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-160 0c-8.8 0-16-7.2-16-16s7.2-16 16-16z"; // 占位符：文件图标
    }
}

public class ArchiveTreeNode : INotifyPropertyChanged
{
    private bool _isExpanded;

    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public ObservableCollection<ArchiveTreeNode> Children { get; set; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum ArchiveType
{
    Unknown,
    Zip,
    SevenZip,
    Rar,
    Tar,
    GZip
}

public partial class ZipViewer : PageMixModelBase, IAurelioTabPage
{
    private readonly string _archivePath;
    private string _currentDirectory = "/";
    private IArchive? _currentArchive;
    private readonly List<ArchiveEntryViewModel> _allEntries = new();
    private bool _isUpdatingTreeSelection = false;
    private string _directoryBeforeReload = "/";

    // Data binding properties
    public ObservableCollection<ArchiveTreeNode> TreeNodes { get; set; } = new();
    public ObservableCollection<ArchiveEntryViewModel> CurrentDirectoryFiles { get; set; } = new();

    private ArchiveTreeNode? _selectedTreeNode;

    public ArchiveTreeNode? SelectedTreeNode
    {
        get => _selectedTreeNode;
        set
        {
            if (SetField(ref _selectedTreeNode, value))
            {
                if (value != null && !_isUpdatingTreeSelection)
                {
                    NavigateToDirectory(value.FullPath);
                }
            }
        }
    }

    private ArchiveEntryViewModel? _selectedFile;

    public ArchiveEntryViewModel? SelectedFile
    {
        get => _selectedFile;
        set => SetField(ref _selectedFile, value);
    }

    public string CurrentPath
    {
        get => _currentDirectory;
        set
        {
            SetField(ref _currentDirectory, value);
            // CanNavigateUp will be automatically updated when CurrentPath changes
        }
    }

    public bool HasSelectedItems => FileListGrid?.SelectedItems?.Count > 0;
    public bool HasSingleSelection => FileListGrid?.SelectedItems?.Count == 1;
    public bool CanNavigateUp => _currentDirectory != "/";

    public ZipViewer(string title, string filePath)
    {
        _archivePath = filePath;
        InitializeComponent();

        PageInfo = new PageInfoEntry
        {
            Title = title,
            Icon = StreamGeometry.Parse(
                "M64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-288-128 0c-17.7 0-32-14.3-32-32L224 0 64 0zM256 0l0 128 128 0L256 0zM96 48c0-8.8 7.2-16 16-16l32 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-32 0c-8.8 0-16-7.2-16-16zm0 64c0-8.8 7.2-16 16-16l32 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-32 0c-8.8 0-16-7.2-16-16zm0 64c0-8.8 7.2-16 16-16l32 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-32 0c-8.8 0-16-7.2-16-16zm-6.3 71.8c3.7-14 16.4-23.8 30.9-23.8l14.8 0c14.5 0 27.2 9.7 30.9 23.8l23.5 88.2c1.4 5.4 2.1 10.9 2.1 16.4c0 35.2-28.8 63.7-64 63.7s-64-28.5-64-63.7c0-5.5 .7-11.1 2.1-16.4l23.5-88.2zM112 336c-8.8 0-16 7.2-16 16s7.2 16 16 16l32 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-32 0z")
        };
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        DataContext = this;

        // Load archive on initialization
        _ = LoadArchiveAsync();

        // Setup event handlers
        FileListGrid.SelectionChanged += OnFileListSelectionChanged;

        // Setup drag & drop handlers
        DropBorder.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        DropBorder.AddHandler(DragDrop.DropEvent, OnDrop);
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
        _currentArchive?.Dispose();
    }

    // Archive Loading and Management
    private async Task LoadArchiveAsync()
    {
        try
        {
            if (!File.Exists(_archivePath))
            {
                Notice($"压缩包文件未找到: {_archivePath}", NotificationType.Error);
                return;
            }

            var archiveType = DetectArchiveType(_archivePath);
            if (archiveType == ArchiveType.Unknown)
            {
                Notice($"不支持的压缩包格式: {Path.GetExtension(_archivePath)}", NotificationType.Error);
                return;
            }

            await Task.Run(() =>
            {
                _currentArchive?.Dispose();

                if (archiveType == ArchiveType.Zip)
                {
                    LoadZipArchive();
                }
                else
                {
                    LoadSharpCompressArchive();
                }
            });

            BuildTreeStructure();

            // Navigate to the directory we were in before reload, or root if it's the first load
            var targetDirectory = _directoryBeforeReload;

            // Check if the target directory still exists after reload
            if (targetDirectory != "/" && !_allEntries.Any(e => e.IsDirectory && e.FullPath == targetDirectory))
            {
                targetDirectory = "/"; // Fall back to root if the directory no longer exists
            }

            NavigateToDirectory(targetDirectory);
            // Notice($"压缩包加载成功: {Path.GetFileName(_archivePath)}", NotificationType.Success);
        }
        catch (Exception ex)
        {
            Notice($"加载压缩包失败: {ex.Message}", NotificationType.Error);
        }
    }

    private ArchiveType DetectArchiveType(string path)
    {
        var extension = Path.GetExtension(path).ToLower();
        return extension switch
        {
            ".zip" => ArchiveType.Zip,
            ".7z" => ArchiveType.SevenZip,
            ".rar" => ArchiveType.Rar,
            ".tar" => ArchiveType.Tar,
            ".gz" => ArchiveType.GZip,
            _ => ArchiveType.Unknown
        };
    }

    private void LoadZipArchive()
    {
        _allEntries.Clear();
        using var archive = ZipFile.OpenRead(_archivePath);

        foreach (var entry in archive.Entries)
        {
            var entryPath = entry.FullName.Replace('\\', '/');
            if (string.IsNullOrEmpty(entryPath)) continue;

            // Skip any ".." entries from the archive itself
            var fileName = Path.GetFileName(entryPath.TrimEnd('/'));
            if (fileName == ".." || fileName == ".")
                continue;

            // Skip directory entries that end with '/' - we'll create them separately
            var isDirectory = entry.FullName.EndsWith('/');
            if (isDirectory && entry.Length == 0)
                continue;

            var normalizedPath = "/" + entryPath.TrimEnd('/').TrimStart('/');
            fileName = isDirectory ? Path.GetFileName(entryPath.TrimEnd('/')) : Path.GetFileName(entryPath);
            _allEntries.Add(new ArchiveEntryViewModel
            {
                Name = fileName,
                FullPath = normalizedPath, // Ensure path starts with /
                Size = entry.Length,
                CompressedSize = entry.CompressedLength,
                Type = isDirectory ? "文件夹" : GetFileType(entryPath),
                IsDirectory = isDirectory,
                LastModified = entry.LastWriteTime.DateTime,
                IconPath = ArchiveEntryViewModel.GetIconPath(fileName, isDirectory)
            });
        }

        // Add implicit directories for files in subdirectories
        var directories = new HashSet<string>();
        var filesToProcess = _allEntries.Where(e => !e.IsDirectory).ToList(); // Create a copy to avoid modification during enumeration

        foreach (var entry in filesToProcess)
        {
            var dir = Path.GetDirectoryName(entry.FullPath)?.Replace('\\', '/');
            while (!string.IsNullOrEmpty(dir))
            {
                if (directories.Add(dir))
                {
                    var normalizedDirPath = "/" + dir.TrimStart('/');
                    var dirName = Path.GetFileName(dir);
                    _allEntries.Add(new ArchiveEntryViewModel
                    {
                        Name = dirName,
                        FullPath = normalizedDirPath,
                        Size = 0,
                        CompressedSize = 0,
                        Type = "文件夹",
                        IsDirectory = true,
                        LastModified = DateTime.MinValue,
                        IconPath = ArchiveEntryViewModel.GetIconPath(dirName, true)
                    });
                }
                dir = Path.GetDirectoryName(dir)?.Replace('\\', '/');
            }
        }
    }

    private void LoadSharpCompressArchive()
    {
        _allEntries.Clear();
        _currentArchive = ArchiveFactory.Open(_archivePath);

        // Add all entries (files and directories)
        foreach (var entry in _currentArchive.Entries)
        {
            var entryPath = entry.Key?.Replace('\\', '/') ?? "";
            if (string.IsNullOrEmpty(entryPath)) continue;

            // Skip any ".." entries from the archive itself
            var fileName = Path.GetFileName(entryPath.TrimEnd('/'));
            if (fileName == ".." || fileName == ".")
                continue;

            var normalizedPath = "/" + entryPath.TrimEnd('/').TrimStart('/');
            fileName = entry.IsDirectory ? Path.GetFileName(entryPath.TrimEnd('/')) : Path.GetFileName(entryPath);
            _allEntries.Add(new ArchiveEntryViewModel
            {
                Name = fileName,
                FullPath = normalizedPath, // Ensure path starts with /
                Size = entry.Size,
                CompressedSize = entry.CompressedSize,
                Type = entry.IsDirectory ? "文件夹" : GetFileType(entryPath),
                IsDirectory = entry.IsDirectory,
                LastModified = entry.LastModifiedTime ?? DateTime.MinValue,
                IconPath = ArchiveEntryViewModel.GetIconPath(fileName, entry.IsDirectory)
            });
        }

        // Add implicit directories for files in subdirectories (if not already present)
        var existingDirs = new HashSet<string>(_allEntries.Where(e => e.IsDirectory).Select(e => e.FullPath));
        var directories = new HashSet<string>();
        var filesToProcess = _allEntries.Where(e => !e.IsDirectory).ToList(); // Create a copy to avoid modification during enumeration

        foreach (var entry in filesToProcess)
        {
            var dir = Path.GetDirectoryName(entry.FullPath)?.Replace('\\', '/');
            while (!string.IsNullOrEmpty(dir))
            {
                var normalizedDirPath = "/" + dir.TrimStart('/');
                if (!existingDirs.Contains(normalizedDirPath) && directories.Add(dir))
                {
                    var dirName = Path.GetFileName(dir);
                    _allEntries.Add(new ArchiveEntryViewModel
                    {
                        Name = dirName,
                        FullPath = normalizedDirPath,
                        Size = 0,
                        CompressedSize = 0,
                        Type = "文件夹",
                        IsDirectory = true,
                        LastModified = DateTime.MinValue,
                        IconPath = ArchiveEntryViewModel.GetIconPath(dirName, true)
                    });
                }
                dir = Path.GetDirectoryName(dir)?.Replace('\\', '/');
            }
        }
    }

    private string GetFileType(string path)
    {
        var extension = Path.GetExtension(path).ToLower();
        return extension switch
        {
            ".txt" => "Text File",
            ".pdf" => "PDF Document",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Image",
            ".mp3" or ".wav" or ".flac" => "Audio",
            ".mp4" or ".avi" or ".mkv" => "Video",
            ".zip" or ".rar" or ".7z" => "Archive",
            ".exe" or ".msi" => "Executable",
            ".dll" => "Library",
            _ => string.IsNullOrEmpty(extension) ? "File" : $"{extension.TrimStart('.')} File"
        };
    }

    // Tree Structure and Navigation
    private void BuildTreeStructure()
    {
        TreeNodes.Clear();
        var rootNode = new ArchiveTreeNode
        {
            Name = Path.GetFileName(_archivePath),
            FullPath = "/",
            IsDirectory = true
        };

        var pathNodes = new Dictionary<string, ArchiveTreeNode> { ["/"] = rootNode };

        foreach (var entry in _allEntries.Where(e => e.IsDirectory).OrderBy(e => e.FullPath))
        {
            // Skip root directory entry
            if (entry.FullPath == "/") continue;

            var parts = entry.FullPath.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "/";
            ArchiveTreeNode parentNode = rootNode;

            for (int i = 0; i < parts.Length; i++)
            {
                currentPath = currentPath == "/" ? $"/{parts[i]}" : $"{currentPath}/{parts[i]}";

                if (!pathNodes.ContainsKey(currentPath))
                {
                    var node = new ArchiveTreeNode
                    {
                        Name = parts[i],
                        FullPath = currentPath,
                        IsDirectory = true
                    };

                    pathNodes[currentPath] = node;
                    parentNode.Children.Add(node);
                }

                parentNode = pathNodes[currentPath];
            }
        }

        TreeNodes.Add(rootNode);
    }

    private void NavigateToDirectory(string path)
    {
        CurrentPath = path;

        // Force clear all items and ensure no ".." entries remain
        CurrentDirectoryFiles.Clear();

        // Double-check: remove any existing ".." entries that might be cached
        var existingDotDot = CurrentDirectoryFiles.Where(f => f.Name == "..").ToList();
        foreach (var item in existingDotDot)
        {
            CurrentDirectoryFiles.Remove(item);
        }

        // Manually trigger CanNavigateUp property change
        var dummy = "";
        SetField(ref dummy, "", nameof(CanNavigateUp));

        // Sync tree view selection to current directory
        SyncTreeViewToCurrentDirectory(path);

        // Add parent directory entry ONLY if not at root directory
        if (path != "/")
        {
            CurrentDirectoryFiles.Add(new ArchiveEntryViewModel
            {
                Name = "..",
                FullPath = GetParentDirectory(path),
                Type = "文件夹",
                IsDirectory = true,
                Size = 0,
                CompressedSize = 0,
                LastModified = DateTime.MinValue,
                IconPath = ArchiveEntryViewModel.GetIconPath("..", true, true) // 返回上级目录
            });
        }

        // Add directories in current path (exclude any ".." entries from archive)
        var currentDirs = _allEntries
            .Where(e => e.IsDirectory && e.Name != ".." && IsDirectChild(e.FullPath, path))
            .OrderBy(e => e.Name);

        foreach (var dir in currentDirs)
        {
            CurrentDirectoryFiles.Add(dir);
        }

        // Add files in current path (exclude any ".." entries from archive)
        var currentFiles = _allEntries
            .Where(e => !e.IsDirectory && e.Name != ".." && IsDirectChild(e.FullPath, path))
            .OrderBy(e => e.Name);

        foreach (var file in currentFiles)
        {
            CurrentDirectoryFiles.Add(file);
        }

        // CanNavigateUp will be notified through CurrentPath property setter

        // Final safety check: remove any ".." entries or empty entries if we're at root
        if (path == "/")
        {
            var problematicEntries = CurrentDirectoryFiles.Where(f =>
                f.Name == ".." ||
                f.Name == "." ||
                string.IsNullOrWhiteSpace(f.Name) ||
                f.Name.Trim() == "").ToList();

            foreach (var entry in problematicEntries)
            {
                CurrentDirectoryFiles.Remove(entry);
            }
        }
    }

    private bool IsDirectChild(string itemPath, string parentPath)
    {
        // Both paths should start with / now
        if (parentPath == "/")
        {
            // For root directory, check if the item path has only one level
            var pathWithoutRoot = itemPath.TrimStart('/');
            return !pathWithoutRoot.Contains('/');
        }

        // For non-root directories, check if item is direct child
        if (!itemPath.StartsWith(parentPath + "/"))
            return false;

        var relativePath = itemPath.Substring(parentPath.Length + 1);
        return !relativePath.Contains('/');
    }

    private string GetParentDirectory(string path)
    {
        if (path == "/" || string.IsNullOrEmpty(path))
            return "/";

        var lastSlash = path.TrimEnd('/').LastIndexOf('/');
        return lastSlash <= 0 ? "/" : path.Substring(0, lastSlash);
    }

    // UI Event Handlers
    public async void ExtractToFolder(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 弹出文件夹选择对话框
            var folderList = await this.PickFolderAsync(new FolderPickerOpenOptions
            {
                Title = "选择解压目标文件夹"
            });

            if (folderList.Count == 0)
                return; // 用户取消了选择

            var selectedFolder = folderList[0];
            var archiveName = Path.GetFileNameWithoutExtension(_archivePath);
            var extractPath = Path.Combine(selectedFolder, archiveName);

            if (Directory.Exists(extractPath))
            {
                var result = await ShowDialogAsync(
                    "解压压缩包",
                    $"文件夹 '{archiveName}' 已存在。是否要覆盖它？",
                    b_primary: "覆盖", b_cancel: "取消");

                if (result != ContentDialogResult.Primary)
                    return;
            }

            Directory.CreateDirectory(extractPath);
            await ExtractAllFiles(extractPath);
            Notice($"压缩包已解压到: {extractPath}", NotificationType.Success);
        }
        catch (Exception ex)
        {
            Notice($"解压失败: {ex.Message}", NotificationType.Error);
        }
    }

    public async void ExtractToCurrent(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 在压缩包同级目录下创建与压缩包相同名称的文件夹
            var archiveName = Path.GetFileNameWithoutExtension(_archivePath);
            var archiveDir = Path.GetDirectoryName(_archivePath) ?? "";
            var extractPath = Path.Combine(archiveDir, archiveName);

            if (Directory.Exists(extractPath))
            {
                var result = await ShowDialogAsync(
                    "解压压缩包",
                    $"文件夹 '{archiveName}' 已存在。是否要覆盖它？",
                    b_primary: "覆盖", b_cancel: "取消");

                if (result != ContentDialogResult.Primary)
                    return;
            }

            Directory.CreateDirectory(extractPath);
            await ExtractAllFiles(extractPath);
            Notice($"压缩包已解压到: {extractPath}", NotificationType.Success);
        }
        catch (Exception ex)
        {
            Notice($"解压失败: {ex.Message}", NotificationType.Error);
        }
    }

    public async void AddFiles(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await this.PickFileAsync(new FilePickerOpenOptions
            {
                Title = "Select files to add to archive",
                AllowMultiple = true
            });

            if (files.Count > 0)
            {
                // Save current directory before reload
                _directoryBeforeReload = CurrentPath;

                await AddFilesToArchive(files);
            }
        }
        catch (Exception ex)
        {
            Notice($"添加文件失败: {ex.Message}", NotificationType.Error);
        }
    }

    public async void DeleteSelected(object? sender, RoutedEventArgs e)
    {
        try
        {
            var selectedItems = FileListGrid.SelectedItems?.Cast<ArchiveEntryViewModel>().ToList();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                Notice("未选择要删除的项目", NotificationType.Warning);
                return;
            }

            var itemNames = string.Join(", ", selectedItems.Select(i => i.Name));
            var result = await ShowDialogAsync(
                "删除项目",
                $"确定要从压缩包中删除以下项目吗？\n\n{itemNames}",
                b_primary:"删除", b_cancel:"取消");

            if (result == ContentDialogResult.Primary)
            {
                // Save current directory before reload
                _directoryBeforeReload = CurrentPath;

                await DeleteItemsFromArchive(selectedItems);
            }
        }
        catch (Exception ex)
        {
            Notice($"删除项目失败: {ex.Message}", NotificationType.Error);
        }
    }

    public void NavigateUp(object? sender, RoutedEventArgs e)
    {
        if (CanNavigateUp)
        {
            NavigateToDirectory(GetParentDirectory(_currentDirectory));
        }
    }

    public async void RefreshArchive(object? sender, RoutedEventArgs e)
    {
        await LoadArchiveAsync();
    }

    public void SaveArchive(object? sender, RoutedEventArgs e)
    {
        try
        {
            // For now, just show a success message since auto-save is already implemented
            // In the future, this could trigger a manual save operation
            Notice("压缩包已保存", NotificationType.Success);
        }
        catch (Exception ex)
        {
            Notice($"保存失败: {ex.Message}", NotificationType.Error);
        }
    }

    // Context Menu Handlers
    public async void ExtractSelected(object? sender, RoutedEventArgs e)
    {
        try
        {
            var selectedItems = FileListGrid.SelectedItems?.Cast<ArchiveEntryViewModel>().ToList();
            if (selectedItems == null || selectedItems.Count == 0) return;

            // Check if selection contains folders or files
            var hasFolders = selectedItems.Any(item => item.IsDirectory);
            var hasFiles = selectedItems.Any(item => !item.IsDirectory);

            string extractPath;

            if (hasFolders && !hasFiles)
            {
                // Only folders selected - show folder picker
                var folderList = await this.PickFolderAsync(new FolderPickerOpenOptions
                {
                    Title = "选择解压目标文件夹"
                });

                if (folderList.Count == 0)
                    return; // User cancelled

                extractPath = folderList[0];
            }
            else if (hasFiles && !hasFolders)
            {
                // Only files selected - show save file dialog for the directory
                var folderList = await this.PickFolderAsync(new FolderPickerOpenOptions
                {
                    Title = "选择文件保存位置"
                });

                if (folderList.Count == 0)
                    return; // User cancelled

                extractPath = folderList[0];
            }
            else
            {
                // Mixed selection - show folder picker
                var folderList = await this.PickFolderAsync(new FolderPickerOpenOptions
                {
                    Title = "选择解压目标文件夹"
                });

                if (folderList.Count == 0)
                    return; // User cancelled

                extractPath = folderList[0];
            }

            await ExtractSelectedFiles(selectedItems, extractPath);
            Notice($"已解压 {selectedItems.Count} 个项目到: {extractPath}", NotificationType.Success);
        }
        catch (Exception ex)
        {
            Notice($"解压文件失败: {ex.Message}", NotificationType.Error);
        }
    }

    public async void RenameSelected(object? sender, RoutedEventArgs e)
    {
        try
        {
            var selectedItem = FileListGrid.SelectedItem as ArchiveEntryViewModel;
            if (selectedItem == null) return;

            var newName = await ShowInputDialogAsync("重命名项目", "输入新名称:", selectedItem.Name);
            if (string.IsNullOrWhiteSpace(newName) || newName == selectedItem.Name) return;

            // Save current directory before reload
            _directoryBeforeReload = CurrentPath;

            await RenameItemInArchive(selectedItem, newName);
        }
        catch (Exception ex)
        {
            Notice($"重命名失败: {ex.Message}", NotificationType.Error);
        }
    }

    public async void ShowProperties(object? sender, RoutedEventArgs e)
    {
        var selectedItem = FileListGrid.SelectedItem as ArchiveEntryViewModel;
        if (selectedItem == null) return;

        var properties = $"名称: {selectedItem.Name}\n" +
                         $"路径: {selectedItem.FullPath}\n" +
                         $"类型: {selectedItem.Type}\n" +
                         $"大小: {selectedItem.SizeFormatted}\n" +
                         $"压缩后: {selectedItem.CompressedSizeFormatted}\n" +
                         $"修改时间: {selectedItem.LastModifiedFormatted}";

        await ShowDialogAsync("属性", properties, b_primary:"确定");
    }

    // Drag & Drop Handlers
    public void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    public async void OnDrop(object? sender, DragEventArgs e)
    {
        try
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null)
                {
                    // Save current directory before reload
                    _directoryBeforeReload = CurrentPath;

                    var filePaths = files.Select(f => f.Path.LocalPath).ToList();
                    await AddFilesToArchive(filePaths);
                }
            }
        }
        catch (Exception ex)
        {
            Notice($"添加拖拽文件失败: {ex.Message}", NotificationType.Error);
        }

        e.Handled = true;
    }

    // Selection synchronization - removed file selection sync
    private void OnFileListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // No longer sync tree selection based on file list selection
        // Tree selection should only sync with current directory navigation
    }

    private void SyncTreeViewToCurrentDirectory(string currentPath)
    {
        // Find the tree node that represents the current directory
        var treeNode = FindTreeNode(TreeNodes, currentPath);
        if (treeNode != null)
        {
            // Temporarily disable the tree selection handler to avoid recursion
            _isUpdatingTreeSelection = true;

            // Expand all parent nodes to make the target node visible
            ExpandPathToNode(treeNode, currentPath);

            // Select the target node
            SelectedTreeNode = treeNode;
            _isUpdatingTreeSelection = false;
        }
        else if (currentPath == "/")
        {
            // If we're at root, select the root node
            _isUpdatingTreeSelection = true;
            SelectedTreeNode = TreeNodes.FirstOrDefault();
            _isUpdatingTreeSelection = false;
        }
    }

    private void SyncTreeViewSelection(string path)
    {
        var treeNode = FindTreeNode(TreeNodes, path);
        if (treeNode != null)
        {
            // Temporarily disable the tree selection handler to avoid recursion
            _isUpdatingTreeSelection = true;
            SelectedTreeNode = treeNode;
            ExpandToNode(treeNode);
            _isUpdatingTreeSelection = false;
        }
    }

    private ArchiveTreeNode? FindTreeNode(ObservableCollection<ArchiveTreeNode> nodes, string path)
    {
        foreach (var node in nodes)
        {
            if (node.FullPath == path)
                return node;

            var childResult = FindTreeNode(node.Children, path);
            if (childResult != null)
                return childResult;
        }
        return null;
    }

    private void ExpandPathToNode(ArchiveTreeNode targetNode, string targetPath)
    {
        // Get all path segments to expand parent nodes
        var pathSegments = targetPath.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentPath = "/";

        // Expand each parent node in the path
        for (int i = 0; i < pathSegments.Length; i++)
        {
            currentPath = currentPath == "/" ? $"/{pathSegments[i]}" : $"{currentPath}/{pathSegments[i]}";
            var nodeToExpand = FindTreeNode(TreeNodes, currentPath);

            if (nodeToExpand != null)
            {
                // Set IsExpanded to true to expand the node
                // Note: This may not work perfectly with all TreeView implementations
                // but it provides the basic functionality
                nodeToExpand.IsExpanded = true;
            }
        }
    }

    private void ExpandToNode(ArchiveTreeNode node)
    {
        // This method is kept for compatibility but now uses ExpandPathToNode
        if (node != null)
        {
            ExpandPathToNode(node, node.FullPath);
        }
    }

    // Double-click navigation
    private void FileListGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var selectedItem = FileListGrid.SelectedItem as ArchiveEntryViewModel;
        if (selectedItem?.IsDirectory == true)
        {
            // NavigateToDirectory will automatically sync tree view to current directory
            NavigateToDirectory(selectedItem.FullPath);
        }
    }

    // Archive Manipulation Methods
    private async Task ExtractAllFiles(string extractPath)
    {
        await Task.Run(() =>
        {
            var archiveType = DetectArchiveType(_archivePath);

            if (archiveType == ArchiveType.Zip)
            {
                ZipFile.ExtractToDirectory(_archivePath, extractPath, true);
            }
            else
            {
                using var archive = ArchiveFactory.Open(_archivePath);
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    entry.WriteToDirectory(extractPath, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        });
    }

    private async Task ExtractSelectedFiles(List<ArchiveEntryViewModel> items, string extractPath)
    {
        await Task.Run(() =>
        {
            // Normalize the extract path to handle drive root directories properly
            extractPath = Path.GetFullPath(extractPath);

            var archiveType = DetectArchiveType(_archivePath);

            if (archiveType == ArchiveType.Zip)
            {
                using var archive = ZipFile.OpenRead(_archivePath);

                foreach (var item in items)
                {
                    if (item.IsDirectory)
                    {
                        // Extract all files in this directory
                        var dirPath = item.FullPath.TrimStart('/');
                        var dirEntries = archive.Entries.Where(e =>
                            !string.IsNullOrEmpty(e.FullName) &&
                            e.FullName.StartsWith(dirPath + "/") &&
                            !e.FullName.EndsWith("/"));

                        foreach (var entry in dirEntries)
                        {
                            try
                            {
                                var relativePath = entry.FullName.Substring(dirPath.Length + 1);
                                var destinationPath = Path.GetFullPath(Path.Combine(extractPath, item.Name, relativePath));

                                // Handle directory creation safely for root drives
                                var directoryPath = Path.GetDirectoryName(destinationPath);
                                if (!string.IsNullOrEmpty(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                // Use manual extraction instead of ExtractToFile
                                using var entryStream = entry.Open();
                                using var fileStream = File.Create(destinationPath);
                                entryStream.CopyTo(fileStream);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"解压文件 '{entry.FullName}' 失败: {ex.Message}", ex);
                            }
                        }
                    }
                    else
                    {
                        // Extract single file
                        var entryPath = item.FullPath.TrimStart('/');
                        var entry = archive.GetEntry(entryPath);
                        if (entry != null)
                        {
                            try
                            {
                                var destinationPath = Path.GetFullPath(Path.Combine(extractPath, item.Name));

                                // Handle directory creation safely for root drives
                                var directoryPath = Path.GetDirectoryName(destinationPath);
                                if (!string.IsNullOrEmpty(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                // Use manual extraction instead of ExtractToFile
                                using var entryStream = entry.Open();
                                using var fileStream = File.Create(destinationPath);
                                entryStream.CopyTo(fileStream);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"解压文件 '{item.Name}' 失败: {ex.Message}", ex);
                            }
                        }
                    }
                }
            }
            else
            {
                using var archive = ArchiveFactory.Open(_archivePath);

                foreach (var item in items)
                {
                    if (item.IsDirectory)
                    {
                        // Extract all files in this directory
                        var dirPath = item.FullPath.TrimStart('/');
                        var dirEntries = archive.Entries.Where(e =>
                            !string.IsNullOrEmpty(e.Key) &&
                            e.Key.StartsWith(dirPath + "/") &&
                            !e.IsDirectory);

                        foreach (var entry in dirEntries)
                        {
                            try
                            {
                                var relativePath = entry.Key.Substring(dirPath.Length + 1);
                                var destinationPath = Path.GetFullPath(Path.Combine(extractPath, item.Name, relativePath));

                                // Handle directory creation safely for root drives
                                var directoryPath = Path.GetDirectoryName(destinationPath);
                                if (!string.IsNullOrEmpty(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                using var entryStream = entry.OpenEntryStream();
                                using var fileStream = File.Create(destinationPath);
                                entryStream.CopyTo(fileStream);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"解压文件 '{entry.Key}' 失败: {ex.Message}", ex);
                            }
                        }
                    }
                    else
                    {
                        // Extract single file
                        var entryPath = item.FullPath.TrimStart('/');
                        var entry = archive.Entries.FirstOrDefault(e => e.Key == entryPath);
                        if (entry != null)
                        {
                            try
                            {
                                var destinationPath = Path.GetFullPath(Path.Combine(extractPath, item.Name));

                                // Handle directory creation safely for root drives
                                var directoryPath = Path.GetDirectoryName(destinationPath);
                                if (!string.IsNullOrEmpty(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                using var entryStream = entry.OpenEntryStream();
                                using var fileStream = File.Create(destinationPath);
                                entryStream.CopyTo(fileStream);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"解压文件 '{item.Name}' 失败: {ex.Message}", ex);
                            }
                        }
                    }
                }
            }
        });
    }

    private async Task AddFilesToArchive(IEnumerable<string> filePaths)
    {
        await Task.Run(() =>
        {
            var archiveType = DetectArchiveType(_archivePath);

            if (archiveType == ArchiveType.Zip)
            {
                using var archive = ZipFile.Open(_archivePath, ZipArchiveMode.Update);
                foreach (var filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        var entryName = Path.Combine(_currentDirectory.TrimStart('/'), Path.GetFileName(filePath))
                            .Replace('\\', '/');

                        // Remove existing entry if it exists
                        var existingEntry = archive.GetEntry(entryName);
                        existingEntry?.Delete();

                        archive.CreateEntryFromFile(filePath, entryName);
                    }
                }
            }
            else
            {
                // For non-ZIP formats, we need to recreate the archive
                // This is a limitation of SharpCompress for some formats
                Notice("Adding files to this archive format requires recreation. This may take a moment.",
                    NotificationType.Information);

                var tempPath = Path.GetTempFileName();
                try
                {
                    using (var newArchive = ArchiveFactory.Create(SharpCompress.Common.ArchiveType.Zip))
                    {
                        // Add existing entries
                        using var oldArchive = ArchiveFactory.Open(_archivePath);
                        foreach (var entry in oldArchive.Entries.Where(e => !e.IsDirectory))
                        {
                            using var stream = entry.OpenEntryStream();
                            newArchive.AddEntry(entry.Key ?? "", stream, true);
                        }

                        // Add new files
                        foreach (var filePath in filePaths)
                        {
                            if (File.Exists(filePath))
                            {
                                var entryName = Path.Combine(_currentDirectory.TrimStart('/'),
                                        Path.GetFileName(filePath))
                                    .Replace('\\', '/');
                                newArchive.AddEntry(entryName, filePath);
                            }
                        }

                        newArchive.SaveTo(tempPath, CompressionType.Deflate);
                    }

                    File.Copy(tempPath, _archivePath, true);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }
        });

        await LoadArchiveAsync();
        Notice($"已向压缩包添加 {filePaths.Count()} 个文件", NotificationType.Success);
    }

    private async Task DeleteItemsFromArchive(List<ArchiveEntryViewModel> items)
    {
        try
        {
            await Task.Run(() =>
            {
                var archiveType = DetectArchiveType(_archivePath);

                if (archiveType == ArchiveType.Zip)
                {
                    try
                    {
                        using var archive = ZipFile.Open(_archivePath, ZipArchiveMode.Update);
                        foreach (var item in items)
                        {
                            if (item.IsDirectory)
                            {
                                // Delete all entries that start with this directory path
                                var entriesToDelete = archive.Entries
                                    .Where(e => e.FullName.StartsWith(item.FullPath.TrimStart('/') + "/"))
                                    .ToList();

                                foreach (var entry in entriesToDelete)
                                {
                                    entry.Delete();
                                }
                            }
                            else
                            {
                                var entry = archive.GetEntry(item.FullPath.TrimStart('/'));
                                entry?.Delete();
                            }
                        }
                    }
                    catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                    {
                        throw new InvalidOperationException("文件被其他程序占用，无法直接修改。请关闭占用该文件的程序，或使用'保存'按钮手动保存更改。", ex);
                    }
                }
            else
            {
                // For non-ZIP formats, recreate the archive without the deleted items
                var tempPath = Path.GetTempFileName();
                var itemsToDelete = new HashSet<string>(items.Select(i => i.FullPath));

                try
                {
                    using (var newArchive = ArchiveFactory.Create(SharpCompress.Common.ArchiveType.Zip))
                    {
                        using var oldArchive = ArchiveFactory.Open(_archivePath);
                        foreach (var entry in oldArchive.Entries.Where(e => !e.IsDirectory))
                        {
                            var shouldDelete = itemsToDelete.Contains(entry.Key ?? "") ||
                                               itemsToDelete.Any(path => entry.Key?.StartsWith(path + "/") == true);

                            if (!shouldDelete)
                            {
                                using var stream = entry.OpenEntryStream();
                                newArchive.AddEntry(entry.Key ?? "", stream, true);
                            }
                        }

                        newArchive.SaveTo(tempPath, CompressionType.Deflate);
                    }

                    File.Copy(tempPath, _archivePath, true);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }
        });

            await LoadArchiveAsync();
            Notice($"已从压缩包中删除 {items.Count} 个项目", NotificationType.Success);
        }
        catch (InvalidOperationException ex)
        {
            Notice(ex.Message, NotificationType.Error);
        }
        catch (Exception ex)
        {
            Notice($"删除失败: {ex.Message}", NotificationType.Error);
        }
    }

    private async Task RenameItemInArchive(ArchiveEntryViewModel item, string newName)
    {
        await Task.Run(() =>
        {
            var archiveType = DetectArchiveType(_archivePath);
            var oldPath = item.FullPath.TrimStart('/');
            var newPath = Path.Combine(Path.GetDirectoryName(oldPath) ?? "", newName).Replace('\\', '/');

            if (archiveType == ArchiveType.Zip)
            {
                using var archive = ZipFile.Open(_archivePath, ZipArchiveMode.Update);

                if (item.IsDirectory)
                {
                    // Handle directory renaming - need to rename all entries that start with this path
                    var oldDirPath = oldPath.EndsWith('/') ? oldPath : oldPath + "/";
                    var newDirPath = newPath.EndsWith('/') ? newPath : newPath + "/";

                    // Get all entries that start with the old directory path
                    var entriesToRename = archive.Entries
                        .Where(e => e.FullName.StartsWith(oldDirPath))
                        .ToList();

                    // Create a list to store new entries data
                    var newEntriesData = new List<(string newName, byte[] data)>();

                    // Read data from old entries
                    foreach (var entry in entriesToRename)
                    {
                        var newEntryName = newDirPath + entry.FullName.Substring(oldDirPath.Length);
                        using var stream = entry.Open();
                        using var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        newEntriesData.Add((newEntryName, memoryStream.ToArray()));
                    }

                    // Delete old entries
                    foreach (var entry in entriesToRename)
                    {
                        entry.Delete();
                    }

                    // Create new entries
                    foreach (var (newEntryName, data) in newEntriesData)
                    {
                        var newEntry = archive.CreateEntry(newEntryName);
                        using var newStream = newEntry.Open();
                        newStream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    // Handle file renaming
                    var entry = archive.GetEntry(oldPath);
                    if (entry != null)
                    {
                        // Create new entry with new name
                        var newEntry = archive.CreateEntry(newPath);
                        using (var oldStream = entry.Open())
                        using (var newStream = newEntry.Open())
                        {
                            oldStream.CopyTo(newStream);
                        }

                        // Delete old entry
                        entry.Delete();
                    }
                }
            }
            else
            {
                // For non-ZIP formats, recreate the archive with renamed entry
                var tempPath = Path.GetTempFileName();

                try
                {
                    using (var newArchive = ArchiveFactory.Create(SharpCompress.Common.ArchiveType.Zip))
                    {
                        using var oldArchive = ArchiveFactory.Open(_archivePath);

                        if (item.IsDirectory)
                        {
                            // Handle directory renaming
                            var oldDirPath = oldPath.EndsWith('/') ? oldPath : oldPath + "/";
                            var newDirPath = newPath.EndsWith('/') ? newPath : newPath + "/";

                            foreach (var entry in oldArchive.Entries.Where(e => !e.IsDirectory))
                            {
                                string entryName;
                                if (entry.Key?.StartsWith(oldDirPath) == true)
                                {
                                    // Rename entries within the directory
                                    entryName = newDirPath + entry.Key.Substring(oldDirPath.Length);
                                }
                                else
                                {
                                    entryName = entry.Key ?? "";
                                }

                                using var stream = entry.OpenEntryStream();
                                newArchive.AddEntry(entryName, stream, true);
                            }
                        }
                        else
                        {
                            // Handle file renaming
                            foreach (var entry in oldArchive.Entries.Where(e => !e.IsDirectory))
                            {
                                var entryName = entry.Key == oldPath ? newPath : entry.Key;
                                using var stream = entry.OpenEntryStream();
                                newArchive.AddEntry(entryName ?? "", stream, true);
                            }
                        }

                        newArchive.SaveTo(tempPath, CompressionType.Deflate);
                    }

                    File.Copy(tempPath, _archivePath, true);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }
        });

        await LoadArchiveAsync();
        // Notice($"已将 '{item.Name}' 重命名为 '{newName}'", NotificationType.Success);
    }

    // Helper Methods
    private async Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "")
    {
        var textBox = new TextBox { Text = defaultValue, Watermark = "请输入值..." };
        var result = await ShowDialogAsync(title, message, textBox, "确定", "取消");
        return result == ContentDialogResult.Primary ? textBox.Text : null;
    }


}