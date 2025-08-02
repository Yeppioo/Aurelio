using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace Aurelio.Views.Main.Pages.Viewers;

public partial class LogViewer : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    private bool _autoScrollToEnd = true;

    private bool _debug = true;

    // 属性字段
    private bool _error = true;
    private bool _exception = true;
    private bool _fatal = true;
    private bool _info = true;
    private bool _stackTrace = true;
    private bool _unknown = true;
    private bool _warning = true;

    public LogViewer(string title)
    {
        InitializeComponent();
        PageInfo = new PageInfoEntry
        {
            Title = title,
            Icon = StreamGeometry.Parse(
                "M192 0c-41.8 0-77.4 26.7-90.5 64L64 64C28.7 64 0 92.7 0 128L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64l-37.5 0C269.4 26.7 233.8 0 192 0zm0 64a32 32 0 1 1 0 64 32 32 0 1 1 0-64zM72 272a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm104-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zM72 368a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm88 0c0-8.8 7.2-16 16-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16z")
        };
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));

        // 直接使用自身作为数据上下文
        DataContext = this;
    }

    public LogViewer()
    {
        InitializeComponent();
    }

    public LogViewer(string filePath, string title)
    {
        InitializeComponent();
        PageInfo = new PageInfoEntry
        {
            Title = title,
            Icon = StreamGeometry.Parse(
                "M192 0c-41.8 0-77.4 26.7-90.5 64L64 64C28.7 64 0 92.7 0 128L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64l-37.5 0C269.4 26.7 233.8 0 192 0zm0 64a32 32 0 1 1 0 64 32 32 0 1 1 0-64zM72 272a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm104-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zM72 368a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm88 0c0-8.8 7.2-16 16-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16z")
        };
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));

        // 直接使用自身作为数据上下文
        DataContext = this;

        // 解析日志文件
        ParseLogFile(filePath);
    }

    // 属性
    public bool Error
    {
        get => _error;
        set
        {
            SetField(ref _error, value);
            UpdateDisplayLogs();
        }
    }

    public bool Info
    {
        get => _info;
        set
        {
            SetField(ref _info, value);
            UpdateDisplayLogs();
        }
    }

    public bool Debug
    {
        get => _debug;
        set
        {
            SetField(ref _debug, value);
            UpdateDisplayLogs();
        }
    }

    public bool Fatal
    {
        get => _fatal;
        set
        {
            SetField(ref _fatal, value);
            UpdateDisplayLogs();
        }
    }

    public bool Warning
    {
        get => _warning;
        set
        {
            SetField(ref _warning, value);
            UpdateDisplayLogs();
        }
    }

    public bool Exception
    {
        get => _exception;
        set
        {
            SetField(ref _exception, value);
            UpdateDisplayLogs();
        }
    }

    public bool StackTrace
    {
        get => _stackTrace;
        set
        {
            SetField(ref _stackTrace, value);
            UpdateDisplayLogs();
        }
    }

    public bool Unknown
    {
        get => _unknown;
        set
        {
            SetField(ref _unknown, value);
            UpdateDisplayLogs();
        }
    }

    public bool AutoScrollToEnd
    {
        get => _autoScrollToEnd;
        set => SetField(ref _autoScrollToEnd, value);
    }

    public ObservableCollection<LogItemEntry> LogItems { get; } = new();
    public ObservableCollection<LogItemEntry> DisplayLogItems { get; } = new();

    private Dictionary<LogType, bool> TypeMap => new()
    {
        { LogType.Error, Error },
        { LogType.Info, Info },
        { LogType.Debug, Debug },
        { LogType.Fatal, Fatal },
        { LogType.Warning, Warning },
        { LogType.Exception, Exception },
        { LogType.StackTrace, StackTrace },
        { LogType.Unknown, Unknown }
    };

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
        Dispose();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var time = DateTime.Now;
        var path = (await TopLevel.GetTopLevel(this).StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "导出日志文件",
                SuggestedFileName = $"{time:yyyy-MM-ddTHH-mm-sszz}.log",
                FileTypeChoices =
                [
                    new FilePickerFileType("Log File") { Patterns = ["*.log"] }
                ]
            }))?.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        await File.WriteAllTextAsync(path,
            $"---- Exported By Aurelio Launcher ----\n" +
            $"---- Exported Time : {time:yyyy-MM-ddTHH:mm:sszzz} ----\n\n\n" +
            $"{string.Join("\n", LogItems.Select(a => a.Original))}");
    }

    public void Dispose()
    {
        LogItems.Clear();
        DisplayLogItems.Clear();
        Control.ItemsSource = null;
        Control.Items.Clear();
        ScrollViewer.Content = null;
    }

    public void AddLog(string source, LogType type, string message)
    {
        var log = new LogItemEntry(source, type, message);
        LogItems.Add(log);

        if (TypeMap[type])
        {
            DisplayLogItems.Add(log);
            if (AutoScrollToEnd) ScrollViewer.ScrollToEnd();
        }
    }

    public void AddLog(LogItemEntry log)
    {
        LogItems.Add(log);

        if (TypeMap[log.Type])
        {
            DisplayLogItems.Add(log);
            if (AutoScrollToEnd) ScrollViewer.ScrollToEnd();
        }
    }

    private void UpdateDisplayLogs()
    {
        DisplayLogItems.Clear();
        foreach (var log in LogItems.Where(log => TypeMap[log.Type])) DisplayLogItems.Add(log);

        if (AutoScrollToEnd) ScrollViewer.ScrollToEnd();
    }

    private void ParseLogFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                AddLog("System", LogType.Error, $"日志文件不存在: {filePath}");
                return;
            }

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var logEntry = ParseLogLine(line);
                if (logEntry != null)
                {
                    AddLog(logEntry);
                }
            }
        }
        catch (Exception ex)
        {
            AddLog("System", LogType.Error, $"读取日志文件失败: {ex.Message}");
        }
    }

    private LogItemEntry? ParseLogLine(string line)
    {
        // 尝试解析用户提供的格式: [2025-07-19 14:53:02.711] [Info] [Thread-1] [Info] [ThreadMain] Launching
        var complexPattern =
            @"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\]\s*\[(\w+)\]\s*\[Thread-\d+\]\s*\[(\w+)\]\s*\[([^\]]+)\]\s*(.*)$";
        var complexMatch = Regex.Match(line, complexPattern);
        if (complexMatch.Success)
        {
            var time = complexMatch.Groups[1].Value;
            var level = complexMatch.Groups[2].Value;
            var source = complexMatch.Groups[4].Value;
            var message = complexMatch.Groups[5].Value;

            return new LogItemEntry(time, source, MapLogLevel(level), message);
        }

        // 尝试解析标准格式: [timestamp] [level] [Thread-id] message
        var standardPattern = @"^\[([^\]]+)\]\s*\[(\w+)\]\s*\[Thread-\d+\]\s*(.*)$";
        var standardMatch = Regex.Match(line, standardPattern);
        if (standardMatch.Success)
        {
            var time = standardMatch.Groups[1].Value;
            var level = standardMatch.Groups[2].Value;
            var message = standardMatch.Groups[3].Value;

            return new LogItemEntry(time, "Application", MapLogLevel(level), message);
        }

        // 尝试解析简单格式: [timestamp] [level] message
        var simplePattern = @"^\[([^\]]+)\]\s*\[(\w+)\]\s*(.*)$";
        var simpleMatch = Regex.Match(line, simplePattern);
        if (simpleMatch.Success)
        {
            var time = simpleMatch.Groups[1].Value;
            var level = simpleMatch.Groups[2].Value;
            var message = simpleMatch.Groups[3].Value;

            return new LogItemEntry(time, "Application", MapLogLevel(level), message);
        }

        // 尝试解析级别开头的格式: INFO: message 或 ERROR: message
        var levelPattern = @"^(DEBUG|INFO|WARN|WARNING|ERROR|FATAL|EXCEPTION):\s*(.*)$";
        var levelMatch = Regex.Match(line, levelPattern, RegexOptions.IgnoreCase);
        if (levelMatch.Success)
        {
            var level = levelMatch.Groups[1].Value;
            var message = levelMatch.Groups[2].Value;

            return new LogItemEntry(DateTime.Now.ToString("HH:mm:ss"), "Application", MapLogLevel(level), message);
        }

        // 如果都不匹配，将整行作为未知类型的消息
        return new LogItemEntry(DateTime.Now.ToString("HH:mm:ss"), "Unknown", LogType.Unknown, line);
    }

    private LogType MapLogLevel(string level)
    {
        return level.ToUpper() switch
        {
            "DEBUG" => LogType.Debug,
            "INFO" => LogType.Info,
            "WARN" or "WARNING" => LogType.Warning,
            "ERROR" => LogType.Error,
            "FATAL" => LogType.Fatal,
            "EXCEPTION" => LogType.Exception,
            "STACKTRACE" => LogType.StackTrace,
            _ => LogType.Unknown
        };
    }

    public static IAurelioNavPage Create((object sender, object? param)t)
    {
        return new LogViewer((string)t. param!, Path.GetFileName((string)t. param!));
    }

    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse(
            "M192 0c-41.8 0-77.4 26.7-90.5 64L64 64C28.7 64 0 92.7 0 128L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64l-37.5 0C269.4 26.7 233.8 0 192 0zm0 64a32 32 0 1 1 0 64 32 32 0 1 1 0-64zM72 272a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm104-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zM72 368a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm88 0c0-8.8 7.2-16 16-16l128 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-128 0c-8.8 0-16-7.2-16-16z"),
        Title = "日志查看器",
        NeedPath = true,
        AutoCreate = false,
        MustPath = true
    };
}