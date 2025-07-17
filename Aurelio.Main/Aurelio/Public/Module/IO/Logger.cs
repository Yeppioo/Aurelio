using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Aurelio.Public.Langs;

namespace Aurelio.Public.Module.IO;

public static class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    private const int MaxLogBackups = 3; // 最多保留的备份数量
    private static readonly object LockObj = new();
    private static string _logFilePath = string.Empty;
    private static bool _initialized;
    private static readonly StringBuilder LogCache = new();

    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Yeppioo.Aurelio", "Logs");

            if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);

            // 保留以前的日志文件，创建带时间戳的备份
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFilePath = Path.Combine(logDirectory, "latest.log");

            if (File.Exists(_logFilePath))
            {
                var backupPath = Path.Combine(logDirectory, $"log_{timestamp}.log");
                try
                {
                    File.Move(_logFilePath, backupPath);
                }
                catch
                {
                    // 如果移动失败，继续使用当前文件
                }
            }

            // 清理旧日志文件，保持备份数量不超过上限
            CleanupOldLogFiles(logDirectory);
            
            const string resourceName = "Aurelio.Version.txt";
            var _assembly = Assembly.GetExecutingAssembly();
            var stream = _assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var result = reader.ReadToEnd();

            var header = $"== Aurelio Log {timestamp} ==\n" +
                         $"Version: v{result.Trim()}\n" +
                         $"OS: {Environment.OSVersion}\n" +
                         $"Runtime: {Environment.Version}\n" +
                         "===============================\n";

            File.WriteAllText(_logFilePath, header);

            _initialized = true;

            // 写入缓存的日志
            if (LogCache.Length > 0)
            {
                File.AppendAllText(_logFilePath, LogCache.ToString());
                LogCache.Clear();
            }

            Info(MainLang.LogSystemInitCompleteTip);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{MainLang.InitLogSystemFailTip}: {ex.Message}");
        }
    }

    /// <summary>
    ///     清理旧的日志备份文件，只保留指定数量的最新备份
    /// </summary>
    private static void CleanupOldLogFiles(string logDirectory)
    {
        try
        {
            // 获取所有备份日志文件
            var backupFiles = Directory.GetFiles(logDirectory, "log_*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            // 如果备份文件数量超过限制，删除最旧的文件
            if (backupFiles.Count > MaxLogBackups)
                foreach (var file in backupFiles.Skip(MaxLogBackups))
                    try
                    {
                        file.Delete();
                        Console.WriteLine($"{MainLang.DeletedOldLogFileTip}: {file.Name}");
                    }
                    catch
                    {
                        // 如果删除失败，继续处理下一个文件
                    }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{MainLang.CleanupOldLogFilesFailTip}: {ex.Message}");
        }
    }

    private static void WriteLog(LogLevel level, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var threadId = Thread.CurrentThread.ManagedThreadId.ToString();
        var logEntry = $"[{timestamp}] [{level}] [Thread-{threadId}] {message}\n";

        try
        {
            if (!_initialized)
            {
                // 如果日志系统尚未初始化，将日志缓存起来
                LogCache.Append(logEntry);
                Console.WriteLine($"[{level}] {message}");
                return;
            }

            lock (LockObj)
            {
                File.AppendAllText(_logFilePath, logEntry);
            }

            // 同时输出到控制台
            Console.WriteLine($"[{level}] {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{MainLang.WriteLogFailTip}: {ex.Message}");
        }
    }

    public static void Debug(string message)
    {
        WriteLog(LogLevel.Debug, message);
    }

    public static void Info(string message)
    {
        WriteLog(LogLevel.Info, message);
    }

    public static void Warning(string message)
    {
        WriteLog(LogLevel.Warning, message);
    }

    public static void Error(string message)
    {
        WriteLog(LogLevel.Error, message);
    }

    public static void Error(Exception ex)
    {
        Error($"{ex.Message}\n{ex.StackTrace}");
    }

    public static void Fatal(string message)
    {
        WriteLog(LogLevel.Fatal, message);
        // Console.WriteLine($"{MainLang.FatalErrorTip}: {message}");
    }

    public static void Fatal(Exception ex)
    {
        var message = $"{ex.Message}\n{ex.StackTrace}";
        if (ex.InnerException != null)
            message += $"\n{MainLang.InnerExceptionTip}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
        Fatal(message);
    }
}