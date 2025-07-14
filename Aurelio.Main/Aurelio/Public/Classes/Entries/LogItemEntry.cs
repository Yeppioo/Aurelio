namespace Aurelio.Public.Classes.Entries;

public enum LogType
{
    Error,
    Info,
    Debug,
    Fatal,
    Warning,
    Exception,
    StackTrace,
    Unknown
}

public class LogItemEntry
{
    public LogItemEntry()
    {
    }

    public LogItemEntry(string source, LogType type, string message)
    {
        Source = source;
        Type = type;
        Message = message;
        Original = $"[{Time}] [{Source}/{Type}] {Message}";
    }

    public string Time { get; set; } = DateTime.Now.ToString("HH:mm:ss");
    public string Source { get; set; } = string.Empty;
    public LogType Type { get; set; } = LogType.Info;
    public string Message { get; set; } = string.Empty;
    public string Original { get; set; } = string.Empty;
}