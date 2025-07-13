using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Aurelio.Public.Module.IO
{
    public static class Logger
    {
        private static readonly object LockObj = new object();
        private static string _logFilePath = string.Empty;
        private static bool _initialized;
        private static readonly StringBuilder LogCache = new StringBuilder();

        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }

        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Yeppioo.Aurelio", "Logs");
                
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 保留以前的日志文件，创建带时间戳的备份
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                _logFilePath = Path.Combine(logDirectory, "latest.log");
                
                if (File.Exists(_logFilePath))
                {
                    string backupPath = Path.Combine(logDirectory, $"log_{timestamp}.log");
                    try
                    {
                        File.Move(_logFilePath, backupPath);
                    }
                    catch
                    {
                        // 如果移动失败，继续使用当前文件
                    }
                }

                // 写入日志头
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                string header = $"=== Aurelio Log {timestamp} ===\n" +
                                $"Version: {version}\n" +
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
                
                Info("日志系统初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化日志系统失败: {ex.Message}");
            }
        }

        private static void WriteLog(LogLevel level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            string logEntry = $"[{timestamp}] [{level}] [Thread-{threadId}] {message}\n";

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
                Console.WriteLine($"写入日志失败: {ex.Message}");
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
            // Console.WriteLine($"致命错误: {message}");
        }

        public static void Fatal(Exception ex)
        {
            string message = $"{ex.Message}\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                message += $"\n内部异常: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }
            Fatal(message);
        }
    }
} 