using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Overlay;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI.Fody.Helpers;
using Ursa.Controls;

namespace Aurelio.Views.Main.Pages;

public partial class NewTabPage : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    // public DateTime _currentTime = DateTime.Now;

    // public string _currentLunarMonthDay = string.Empty;
    // public string _currentLunarYear = string.Empty;
    // public string _currentWeekDay = string.Empty;
    // public string _poem = "loading";
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }

    public Control BottomElement { get; set; }
    // public DateTime CurrentTime
    // {
    //     get => _currentTime;
    //     set
    //     {
    //         SetField(ref _currentTime, value);
    //         // UpdateLunarDate(CurrentTime);
    //         // UpdateWeekDay(CurrentTime);
    //     }
    // }

    //
    // public string CurrentLunarMonthDay
    // {
    //     get => _currentLunarMonthDay;
    //     set => SetField(ref _currentLunarMonthDay, value);
    // }
    //
    // public string Poem
    // {
    //     get => _poem;
    //     set => SetField(ref _poem, value);
    // }
    //
    // public string CurrentLunarYear
    // {
    //     get => _currentLunarYear;
    //     set => SetField(ref _currentLunarYear, value);
    // }
    //
    // public string CurrentWeekDay
    // {
    //     get => _currentWeekDay;
    //     set => SetField(ref _currentWeekDay, value);
    // }
    //
    private bool _fl = true;
    // private bool _suppressNextFilter = false;
    //
    // // File system search properties
    // [Reactive]
    // public StreamGeometry SearchIconData { get; set; } = StreamGeometry.Parse(
    //     "M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z");
    //
    // [Reactive] public bool IsFileSystemMode { get; set; }
    // [Reactive] public string CurrentFileSystemPath { get; set; } = string.Empty;

    public NewTabPage()
    {
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Title = MainLang.NewTab,
            Icon = StreamGeometry.Parse(
                "F1 M 12.670898 5.825195 L 15 6.796875 L 15.97168 9.125977 C 16.025391 9.228516 16.132812 9.296875 16.25 9.296875 C 16.367188 9.296875 16.474609 9.228516 16.52832 9.125977 L 17.5 6.796875 L 19.829102 5.825195 C 19.931641 5.771484 20 5.664062 20 5.546875 C 20 5.429688 19.931641 5.322266 19.829102 5.268555 L 17.5 4.296875 L 16.52832 1.967773 C 16.474609 1.865234 16.367188 1.796875 16.25 1.796875 C 16.132812 1.796875 16.025391 1.865234 15.97168 1.967773 L 15 4.296875 L 12.670898 5.268555 C 12.568359 5.322266 12.5 5.429688 12.5 5.546875 C 12.5 5.664062 12.568359 5.771484 12.670898 5.825195 Z M 19.829102 17.768555 L 17.5 16.796875 L 16.52832 14.467773 C 16.474609 14.365234 16.367188 14.296875 16.25 14.296875 C 16.132812 14.296875 16.025391 14.365234 15.97168 14.467773 L 15 16.796875 L 12.670898 17.768555 C 12.568359 17.822266 12.5 17.929688 12.5 18.046875 C 12.5 18.164062 12.568359 18.271484 12.670898 18.325195 L 15 19.296875 L 15.97168 21.625977 C 16.025391 21.728516 16.132812 21.796875 16.25 21.796875 C 16.367188 21.796875 16.474609 21.728516 16.52832 21.625977 L 17.5 19.296875 L 19.829102 18.325195 C 19.931641 18.271484 20 18.164062 20 18.046875 C 20 17.929688 19.931641 17.822266 19.829102 17.768555 Z M 15 11.782227 C 15 11.547852 14.868164 11.328125 14.65332 11.220703 L 10.258789 9.018555 L 8.056641 4.614258 C 7.84668 4.189453 7.15332 4.189453 6.943359 4.614258 L 4.741211 9.018555 L 0.34668 11.220703 C 0.131836 11.328125 0 11.547852 0 11.782227 C 0 12.021484 0.131836 12.236328 0.34668 12.34375 L 4.741211 14.545898 L 6.943359 18.950195 C 7.045898 19.160156 7.265587 19.296875 7.5 19.296875 C 7.734337 19.296875 7.954102 19.160156 8.056641 18.950195 L 10.258789 14.545898 L 14.65332 12.34375 C 14.868164 12.236328 15 12.021484 15 11.782227 Z ")
        };
        ShortInfo = MainLang.NewTab;
        DataContext = this;
        // var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
        // timer.Tick += (_, _) => CurrentTime = DateTime.Now;
        // timer.Start();
        Loaded += (_, _) =>
        {
            if (!_fl) return;
            _fl = false;
            // Filter();
            // Poem = await Public.Module.IO.Http.Poem.GetPoem();
        };
        // SearchBox.GotFocus += (_, _) =>
        // {
        //     if (_suppressNextFilter)
        //     {
        //         _suppressNextFilter = false;
        //         return;
        //     }
        //
        //     Filter();
        // };
        // AggregateSearchListBox.SelectionChanged += (s, _) =>
        // {
        //     if (AggregateSearchListBox.SelectedItem is not AggregateSearchEntry entry) return;
        //
        //     if (entry.Type is not AggregateSearchEntryType t)
        //     {
        //         AggregateSearch.Execute(entry, this);
        //         return;
        //     }
        //     if (t is AggregateSearchEntryType.SystemFile or AggregateSearchEntryType.SystemFileGoUp)
        //     {
        //         HandleFileSystemEntrySelection(entry);
        //         return;
        //     }
        //
        //     if (t == AggregateSearchEntryType.AurelioTabPage)
        //     {
        //         IRenderRoot root = null;
        //         AggregateSearch.Execute(entry, this, ref root);
        //         HostTab.Close(root);
        //     }
        // };
        //
        // // Add file system search event handlers
        // SearchBox.KeyDown += SearchBox_KeyDown;
        // SearchIcon.PointerPressed += SearchIcon_PointerPressed;
        // AggregateSearchListBox.PointerPressed += AggregateSearchListBox_PointerPressed;
    }

    // private string _aggregateSearchFilter = "";
    // public ObservableCollection<AggregateSearchEntry> FilteredAggregateSearchEntries { get; } = [];

    // public string AggregateSearchFilter
    // {
    //     get => _aggregateSearchFilter;
    //     set
    //     {
    //         SetField(ref _aggregateSearchFilter, value);
    //         Filter();
    //     }
    // }

    // private void Filter()
    // {
    //     try
    //     {
    //         FilteredAggregateSearchEntries.Clear();
    //
    //         // Check if we're in file system search mode
    //         if (AggregateSearchFilter.StartsWith("!!"))
    //         {
    //             IsFileSystemMode = true;
    //             var pathPart = AggregateSearchFilter.Substring(2);
    //             CurrentFileSystemPath = pathPart;
    //
    //             // Update search icon based on whether we have a valid path
    //             UpdateSearchIcon();
    //
    //             // Get file system entries
    //             var fileSystemEntries = GetFileSystemEntries(pathPart);
    //             FilteredAggregateSearchEntries.AddRange(fileSystemEntries);
    //         }
    //         else
    //         {
    //             IsFileSystemMode = false;
    //             CurrentFileSystemPath = string.Empty;
    //             SearchIconData = StreamGeometry.Parse(
    //                 "M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z");
    //
    //             // Normal search mode
    //             FilteredAggregateSearchEntries.AddRange(Data.AggregateSearchEntries.Where(item =>
    //                     item.Title.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase) ||
    //                     item.Summary.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase))
    //                 .OrderByDescending(x => x.Order).ThenBy(x => x.Title));
    //         }
    //     }
    //     catch
    //     {
    //         // ignored
    //     }
    // }
    //
    // private void UpdateLunarDate(DateTime date)
    // {
    //     var chineseEra = new ChineseLunisolarCalendar();
    //     int month = chineseEra.GetMonth(date);
    //     int day = chineseEra.GetDayOfMonth(date);
    //     int year = chineseEra.GetYear(date);
    //
    //     CurrentLunarMonthDay = ToChineseMonth(month) + ToChineseDay(day);
    //     CurrentLunarYear = ToChineseYear(year);
    // }
    //
    // private void UpdateWeekDay(DateTime date)
    // {
    //     string[] weekDays = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];
    //     CurrentWeekDay = weekDays[(int)date.DayOfWeek];
    // }
    //
    //
    // private static string ToChineseMonth(int month)
    // {
    //     string[] months = ["正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "腊"];
    //     return month is >= 1 and <= 12 ? months[month - 1] + "月" : "";
    // }
    //
    // private static string ToChineseDay(int day)
    // {
    //     if (day is >= 1 and <= 10)
    //         return "初" + "一二三四五六七八九十"[day - 1];
    //     if (day == 20)
    //         return "二十";
    //     if (day is >= 21 and <= 29)
    //         return "廿" + "一二三四五六七八九"[day - 21];
    //     return day == 30 ? "三十" : "";
    // }
    //
    // private static string ToChineseYear(int lunarYear)
    // {
    //     // 天干：甲乙丙丁戊己庚辛壬癸
    //     string[] heavenlyStems = ["甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸"];
    //     // 地支：子丑寅卯辰巳午未申酉戌亥
    //     string[] earthlyBranches = ["子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥"];
    //
    //     // 农历年份从1开始，对应甲子年
    //     var stemIndex = (lunarYear - 1) % 10;
    //     var branchIndex = (lunarYear - 1) % 12;
    //
    //     return heavenlyStems[stemIndex] + earthlyBranches[branchIndex] + "年";
    // }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    // #region File System Search Methods
    //
    // private void UpdateSearchIcon()
    // {
    //     if (IsFileSystemMode && !string.IsNullOrEmpty(CurrentFileSystemPath) &&
    //         (Directory.Exists(CurrentFileSystemPath) || File.Exists(CurrentFileSystemPath)))
    //     {
    //         // Show right arrow icon when we have a valid path
    //         SearchIconData = Icons.ChevronRight;
    //     }
    //     else
    //     {
    //         // Show search icon
    //         SearchIconData = StreamGeometry.Parse(
    //             "M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z");
    //     }
    // }
    //
    // private ObservableCollection<AggregateSearchEntry> GetFileSystemEntries(string pathPart)
    // {
    //     var entries = new ObservableCollection<AggregateSearchEntry>();
    //
    //     try
    //     {
    //         if (string.IsNullOrEmpty(pathPart))
    //         {
    //             // Show system roots
    //             entries.AddRange(GetSystemRoots());
    //         }
    //         else
    //         {
    //             // Handle partial path matching
    //             var normalizedPath = pathPart.Replace('/', Path.DirectorySeparatorChar)
    //                 .Replace('\\', Path.DirectorySeparatorChar);
    //
    //             // If the path exists as a complete directory, show its contents
    //             if (Directory.Exists(normalizedPath))
    //             {
    //                 // Add "go up" entry if not at root level
    //                 var goUpEntry = GetGoUpEntry(normalizedPath);
    //                 if (goUpEntry != null)
    //                 {
    //                     entries.Add(goUpEntry);
    //                 }
    //
    //                 entries.AddRange(GetDirectoryContents(normalizedPath));
    //             }
    //             else
    //             {
    //                 // Handle partial path matching
    //                 var parentPath = Path.GetDirectoryName(normalizedPath);
    //                 var partialName = Path.GetFileName(normalizedPath);
    //
    //                 if (!string.IsNullOrEmpty(parentPath) && Directory.Exists(parentPath))
    //                 {
    //                     // Show matching items from parent directory
    //                     entries.AddRange(GetPartialMatchingContents(parentPath, partialName));
    //                 }
    //                 else if (string.IsNullOrEmpty(parentPath))
    //                 {
    //                     // Handle root-level partial matching (e.g., "/h" should match "/home")
    //                     entries.AddRange(GetPartialMatchingRoots(partialName));
    //                 }
    //             }
    //         }
    //     }
    //     catch
    //     {
    //         // Handle permission errors gracefully
    //     }
    //
    //     return entries;
    // }
    //
    // private IEnumerable<AggregateSearchEntry> GetSystemRoots()
    // {
    //     var roots = new List<AggregateSearchEntry>();
    //
    //     try
    //     {
    //         if (Data.DesktopType == DesktopType.Windows)
    //         {
    //             // Windows: Show drive letters
    //             var drives = DriveInfo.GetDrives();
    //             foreach (var drive in drives)
    //             {
    //                 if (drive.IsReady)
    //                 {
    //                     roots.Add(new AggregateSearchEntry(drive.RootDirectory, true));
    //                 }
    //             }
    //         }
    //         else if (Data.DesktopType == DesktopType.Linux)
    //         {
    //             // Linux: Show root and common mount points
    //             var commonPaths = new[] { "/", "/home", "/usr", "/var", "/opt", "/tmp" };
    //             foreach (var path in commonPaths)
    //             {
    //                 if (Directory.Exists(path))
    //                 {
    //                     roots.Add(new AggregateSearchEntry(new DirectoryInfo(path), true));
    //                 }
    //             }
    //         }
    //         else if (Data.DesktopType == DesktopType.MacOs)
    //         {
    //             // macOS: Show root and common directories
    //             var commonPaths = new[] { "/", "/Users", "/Applications", "/System", "/Library" };
    //             foreach (var path in commonPaths)
    //             {
    //                 if (Directory.Exists(path))
    //                 {
    //                     roots.Add(new AggregateSearchEntry(new DirectoryInfo(path), true));
    //                 }
    //             }
    //         }
    //     }
    //     catch
    //     {
    //         // Handle errors gracefully
    //     }
    //
    //     return roots;
    // }
    //
    // private IEnumerable<AggregateSearchEntry> GetDirectoryContents(string directoryPath)
    // {
    //     var contents = new List<AggregateSearchEntry>();
    //
    //     try
    //     {
    //         var directory = new DirectoryInfo(directoryPath);
    //
    //         // Add subdirectories first
    //         var subdirectories = directory.GetDirectories().Take(50); // Limit to 50 items for performance
    //         foreach (var subdir in subdirectories)
    //         {
    //             try
    //             {
    //                 contents.Add(new AggregateSearchEntry(subdir));
    //             }
    //             catch
    //             {
    //                 // Skip directories we can't access
    //             }
    //         }
    //
    //         // Add files
    //         var files = directory.GetFiles().Take(50); // Limit to 50 items for performance
    //         foreach (var file in files)
    //         {
    //             try
    //             {
    //                 contents.Add(new AggregateSearchEntry(file));
    //             }
    //             catch
    //             {
    //                 // Skip files we can't access
    //             }
    //         }
    //     }
    //     catch
    //     {
    //         // Handle permission errors gracefully
    //     }
    //
    //     return contents;
    // }
    //
    // private IEnumerable<AggregateSearchEntry> GetPartialMatchingContents(string parentPath, string partialName)
    // {
    //     var contents = new List<AggregateSearchEntry>();
    //
    //     try
    //     {
    //         var directory = new DirectoryInfo(parentPath);
    //
    //         // Add matching subdirectories
    //         var subdirectories = directory.GetDirectories()
    //             .Where(d => d.Name.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
    //             .Take(50);
    //
    //         foreach (var subdir in subdirectories)
    //         {
    //             try
    //             {
    //                 contents.Add(new AggregateSearchEntry(subdir));
    //             }
    //             catch
    //             {
    //                 // Skip directories we can't access
    //             }
    //         }
    //
    //         // Add matching files
    //         var files = directory.GetFiles()
    //             .Where(f => f.Name.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
    //             .Take(50);
    //
    //         foreach (var file in files)
    //         {
    //             try
    //             {
    //                 contents.Add(new AggregateSearchEntry(file));
    //             }
    //             catch
    //             {
    //                 // Skip files we can't access
    //             }
    //         }
    //     }
    //     catch
    //     {
    //         // Handle permission errors gracefully
    //     }
    //
    //     return contents;
    // }
    //
    // private IEnumerable<AggregateSearchEntry> GetPartialMatchingRoots(string partialName)
    // {
    //     var roots = new List<AggregateSearchEntry>();
    //
    //     try
    //     {
    //         if (Data.DesktopType == DesktopType.Windows)
    //         {
    //             // Windows: Show matching drive letters
    //             var drives = DriveInfo.GetDrives()
    //                 .Where(d => d.IsReady && d.Name.StartsWith(partialName, StringComparison.OrdinalIgnoreCase));
    //
    //             foreach (var drive in drives)
    //             {
    //                 roots.Add(new AggregateSearchEntry(drive.RootDirectory, true));
    //             }
    //         }
    //         else
    //         {
    //             // Linux/macOS: Show matching root directories
    //             var commonPaths = Data.DesktopType == DesktopType.Linux
    //                 ? new[] { "/", "/home", "/usr", "/var", "/opt", "/tmp" }
    //                 : new[] { "/", "/Users", "/Applications", "/System", "/Library" };
    //
    //             foreach (var path in commonPaths)
    //             {
    //                 if (Directory.Exists(path))
    //                 {
    //                     var dirName = path == "/" ? "/" : Path.GetFileName(path);
    //                     if (dirName.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
    //                     {
    //                         roots.Add(new AggregateSearchEntry(new DirectoryInfo(path), true));
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     catch
    //     {
    //         // Handle errors gracefully
    //     }
    //
    //     return roots;
    // }
    //
    // private AggregateSearchEntry? GetGoUpEntry(string currentPath)
    // {
    //     try
    //     {
    //         // Normalize the current path
    //         var normalizedPath = currentPath.TrimEnd(Path.DirectorySeparatorChar, '/');
    //
    //         // Check if we're at drive root level (e.g., C:\ on Windows or / on Unix)
    //         if (Data.DesktopType == DesktopType.Windows)
    //         {
    //             var driveRoot = Path.GetPathRoot(normalizedPath);
    //             if (string.Equals(normalizedPath, driveRoot?.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
    //             {
    //                 // At drive root (e.g., C:\) - go up to drive selection (empty path)
    //                 return new AggregateSearchEntry("");
    //             }
    //         }
    //         else if (Data.DesktopType == DesktopType.Linux || Data.DesktopType == DesktopType.MacOs)
    //         {
    //             if (normalizedPath == "/" || string.IsNullOrEmpty(normalizedPath))
    //             {
    //                 // At system root (/) - go up to drive selection (empty path)
    //                 return new AggregateSearchEntry("");
    //             }
    //         }
    //
    //         // For subdirectories, get the actual parent directory
    //         var parentPath = Path.GetDirectoryName(normalizedPath);
    //
    //         // If parentPath is null or empty, we're at a root level
    //         if (string.IsNullOrEmpty(parentPath))
    //         {
    //             return new AggregateSearchEntry("");
    //         }
    //
    //         return new AggregateSearchEntry(parentPath);
    //     }
    //     catch
    //     {
    //         return null;
    //     }
    // }
    //
    // #endregion
    //
    // #region Event Handlers
    //
    // private void HandleFileSystemEntrySelection(AggregateSearchEntry entry)
    // {
    //     // Handle "go up" entry
    //     if (entry is { Type: AggregateSearchEntryType.SystemFileGoUp, OriginObject: string parentPath })
    //     {
    //         string newPath;
    //
    //         if (string.IsNullOrEmpty(parentPath))
    //         {
    //             // Go back to drive selection
    //             newPath = "!!";
    //         }
    //         else
    //         {
    //             // Navigate to parent directory
    //             newPath = "!!" + parentPath;
    //             if (!newPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
    //             {
    //                 newPath += Path.DirectorySeparatorChar;
    //             }
    //         }
    //
    //         // Update the search filter which will trigger the Filter() method
    //         AggregateSearchFilter = newPath;
    //
    //         // Give focus to the search box and move cursor to end
    //         SearchBox.Focus();
    //         SearchBox.CaretIndex = SearchBox.Text?.Length ?? 0;
    //         return;
    //     }
    //
    //     // Handle regular file system entries
    //     if (entry.OriginObject is not FileSystemInfo fsInfo) return;
    //
    //     if (fsInfo is DirectoryInfo)
    //     {
    //         // Directory: Navigate to this path
    //         var newPath = "!!" + fsInfo.FullName;
    //         if (!newPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
    //         {
    //             newPath += Path.DirectorySeparatorChar;
    //         }
    //
    //         // Update the search filter which will trigger the Filter() method
    //         AggregateSearchFilter = newPath;
    //
    //         // Give focus to the search box and move cursor to end
    //         SearchBox.Focus();
    //         SearchBox.CaretIndex = SearchBox.Text?.Length ?? 0;
    //     }
    //     else if (fsInfo is FileInfo)
    //     {
    //         // File: Open directly with system default application
    //         OpenFileSystemPath(fsInfo.FullName);
    //
    //         // Give focus back to the search box without triggering filter
    //         // Use a flag to prevent the GotFocus event from calling Filter()
    //         _suppressNextFilter = true;
    //         SearchBox.Focus();
    //         SearchBox.CaretIndex = SearchBox.Text?.Length ?? 0;
    //     }
    // }
    //
    // private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    // {
    //     if (e.Key == Key.Enter && IsFileSystemMode && !string.IsNullOrEmpty(CurrentFileSystemPath))
    //     {
    //         OpenFileSystemPath(CurrentFileSystemPath);
    //     }
    // }
    //
    // private void SearchIcon_PointerPressed(object? sender, PointerPressedEventArgs e)
    // {
    //     if (IsFileSystemMode && !string.IsNullOrEmpty(CurrentFileSystemPath))
    //     {
    //         OpenFileSystemPath(CurrentFileSystemPath);
    //     }
    // }
    //
    // private void AggregateSearchListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    // {
    //     if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed ||
    //         AggregateSearchListBox.SelectedItem is not AggregateSearchEntry entry ||
    //         entry.Type is not AggregateSearchEntryType.SystemFile) return;
    //     // Right-click: Open with system default
    //     if (entry.OriginObject is FileSystemInfo fsInfo)
    //     {
    //         OpenFileSystemPath(fsInfo.FullName);
    //     }
    // }
    //
    // private async void OpenFileSystemPath(string path)
    // {
    //     try
    //     {
    //         if (File.Exists(path))
    //         {
    //             var isNav = FileNav.NavPage(path);
    //             if (isNav)
    //             {
    //                 HostTab.Close();
    //                 return;
    //             }
    //             // Open file with default application
    //             var launcher = TopLevel.GetTopLevel(this)?.Launcher;
    //             if (launcher != null)
    //             {
    //                 _ = launcher.LaunchFileInfoAsync(new FileInfo(path));
    //             }
    //         }
    //         else if (Directory.Exists(path))
    //         {
    //             // Open folder with default file manager
    //             await OpenFolder(path);
    //         }
    //     }
    //     catch
    //     {
    //         // Handle errors gracefully
    //     }
    // }
    //
    // #endregion

    // private async void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    // {
    //     if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
    //     {
    //         Poem = "loading";
    //         PoemRoot.IsEnabled = false;
    //         Poem = await Public.Module.IO.Http.Poem.GetPoem();
    //         PoemRoot.IsEnabled = true;
    //     }
    //     else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
    //     {
    //         await App.TopLevel.Clipboard.SetTextAsync(Poem);
    //         Notice(MainLang.AlreadyCopyToClipBoard, NotificationType.Success);
    //     }
    // }

    public void OpenNewPageCommand()
    {
        if (this.GetVisualRoot() is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new PageSelector()));
            return;
        }

        App.UiRoot.CreateTab(new TabEntry(new PageSelector()));
        HostTab.Close();
    }

    public void AggSearchCommand()
    {
        var options = new DialogOptions()
        {
            ShowInTaskBar = false,
            IsCloseButtonVisible = true,
            StartupLocation = WindowStartupLocation.Manual,
            CanDragMove = true,
            StyleClass = "aggregate-search"
        };
        Dialog.ShowCustom<AggregateSearchDialog, AggregateSearchDialog>(new AggregateSearchDialog(),
            this.GetVisualRoot() as Window, options: options);
    }

    public async Task OpenFileCommand()
    {
        var hs = false;
        var fs = await this.PickFileAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            Title = MainLang.OpenFile,
        });
        foreach (var f in fs)
        {
            var isNav = await FileNav.NavPage(f, this.GetVisualRoot() as IAurelioWindow);
            if (isNav)
            {
                hs = true;
                continue;
            }

            Notice($"{MainLang.UnsupportedFileType} {Path.GetExtension(f)}", NotificationType.Error);
        }

        if (hs) HostTab.Close();
    }


    public void OpenInNewWindowCommand()
    {
        HostTab.MoveTabToNewWindow();
    }


    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse(
            "F1 M 12.670898 5.825195 L 15 6.796875 L 15.97168 9.125977 C 16.025391 9.228516 16.132812 9.296875 16.25 9.296875 C 16.367188 9.296875 16.474609 9.228516 16.52832 9.125977 L 17.5 6.796875 L 19.829102 5.825195 C 19.931641 5.771484 20 5.664062 20 5.546875 C 20 5.429688 19.931641 5.322266 19.829102 5.268555 L 17.5 4.296875 L 16.52832 1.967773 C 16.474609 1.865234 16.367188 1.796875 16.25 1.796875 C 16.132812 1.796875 16.025391 1.865234 15.97168 1.967773 L 15 4.296875 L 12.670898 5.268555 C 12.568359 5.322266 12.5 5.429688 12.5 5.546875 C 12.5 5.664062 12.568359 5.771484 12.670898 5.825195 Z M 19.829102 17.768555 L 17.5 16.796875 L 16.52832 14.467773 C 16.474609 14.365234 16.367188 14.296875 16.25 14.296875 C 16.132812 14.296875 16.025391 14.365234 15.97168 14.467773 L 15 16.796875 L 12.670898 17.768555 C 12.568359 17.822266 12.5 17.929688 12.5 18.046875 C 12.5 18.164062 12.568359 18.271484 12.670898 18.325195 L 15 19.296875 L 15.97168 21.625977 C 16.025391 21.728516 16.132812 21.796875 16.25 21.796875 C 16.367188 21.796875 16.474609 21.728516 16.52832 21.625977 L 17.5 19.296875 L 19.829102 18.325195 C 19.931641 18.271484 20 18.164062 20 18.046875 C 20 17.929688 19.931641 17.822266 19.829102 17.768555 Z M 15 11.782227 C 15 11.547852 14.868164 11.328125 14.65332 11.220703 L 10.258789 9.018555 L 8.056641 4.614258 C 7.84668 4.189453 7.15332 4.189453 6.943359 4.614258 L 4.741211 9.018555 L 0.34668 11.220703 C 0.131836 11.328125 0 11.547852 0 11.782227 C 0 12.021484 0.131836 12.236328 0.34668 12.34375 L 4.741211 14.545898 L 6.943359 18.950195 C 7.045898 19.160156 7.265587 19.296875 7.5 19.296875 C 7.734337 19.296875 7.954102 19.160156 8.056641 18.950195 L 10.258789 14.545898 L 14.65332 12.34375 C 14.868164 12.236328 15 12.021484 15 11.782227 Z"),
        Title = MainLang.NewTab,
        NeedPath = false,
        AutoCreate = true
    };

    public static IAurelioNavPage Create((object sender, object? param) t)
    {
        var root = ((Control)t.sender).GetVisualRoot();
        if (root is TabWindow tabWindow)
        {
            tabWindow.CreateTab(new TabEntry(new NewTabPage()));
            return null;
        }

        App.UiRoot.CreateTab(new TabEntry(new NewTabPage()));
        return null;
    }
}