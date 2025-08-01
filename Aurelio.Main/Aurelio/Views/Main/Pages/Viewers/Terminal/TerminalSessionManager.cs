using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aurelio.Views.Main.Pages.Viewers.Terminal;

/// <summary>
/// 终端会话管理器
/// </summary>
public class TerminalSessionManager
{
    private readonly Dictionary<string, TerminalSession> _sessions = new();
    private string? _activeSessionId;

    public event EventHandler<SessionEventArgs>? SessionCreated;
    public event EventHandler<SessionEventArgs>? SessionStopped;
    public event EventHandler<SessionEventArgs>? SessionActivated;

    /// <summary>
    /// 获取当前活动会话
    /// </summary>
    public TerminalSession? ActiveSession => 
        _activeSessionId != null && _sessions.TryGetValue(_activeSessionId, out var session) ? session : null;

    /// <summary>
    /// 获取所有会话
    /// </summary>
    public IEnumerable<TerminalSession> AllSessions => _sessions.Values;

    /// <summary>
    /// 创建新会话
    /// </summary>
    public async Task<TerminalSession?> CreateSessionAsync(string name, string executablePath)
    {
        try
        {
            var session = new TerminalSession(name, executablePath);
            
            if (await session.StartAsync())
            {
                _sessions[session.Id] = session;
                
                // 如果是第一个会话，自动激活
                if (_activeSessionId == null)
                {
                    _activeSessionId = session.Id;
                    session.IsActive = true;
                }

                SessionCreated?.Invoke(this, new SessionEventArgs(session));
                return session;
            }
            else
            {
                session.Dispose();
                return null;
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 切换到指定会话
    /// </summary>
    public bool SwitchToSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return false;

        // 取消当前活动会话
        if (_activeSessionId != null && _sessions.TryGetValue(_activeSessionId, out var currentSession))
        {
            currentSession.IsActive = false;
        }

        // 激活新会话
        _activeSessionId = sessionId;
        session.IsActive = true;
        session.LastActiveTime = DateTime.Now;

        SessionActivated?.Invoke(this, new SessionEventArgs(session));
        return true;
    }

    /// <summary>
    /// 停止指定会话
    /// </summary>
    public bool StopSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return false;

        session.Stop();
        _sessions.Remove(sessionId);

        // 如果停止的是活动会话，切换到其他会话
        if (_activeSessionId == sessionId)
        {
            _activeSessionId = null;
            var nextSession = _sessions.Values.FirstOrDefault(s => s.Status == SessionStatus.Running);
            if (nextSession != null)
            {
                SwitchToSession(nextSession.Id);
            }
        }

        SessionStopped?.Invoke(this, new SessionEventArgs(session));
        session.Dispose();
        return true;
    }

    /// <summary>
    /// 根据ID获取会话
    /// </summary>
    public TerminalSession? GetSession(string sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session : null;
    }

    /// <summary>
    /// 根据名称查找会话
    /// </summary>
    public TerminalSession? FindSessionByName(string name)
    {
        return _sessions.Values.FirstOrDefault(s => 
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 清理所有会话
    /// </summary>
    public void CleanupAllSessions()
    {
        foreach (var session in _sessions.Values.ToList())
        {
            session.Dispose();
        }
        _sessions.Clear();
        _activeSessionId = null;
    }

    /// <summary>
    /// 从环境变量PATH中查找可执行文件
    /// </summary>
    public static string? FindExecutableInPath(string executableName)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
            return null;

        var paths = pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        // 在Windows上，可执行文件可能有多种扩展名
        var extensions = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? new[] { ".exe", ".cmd", ".bat", ".com" }
            : new[] { "" };

        foreach (var path in paths)
        {
            try
            {
                foreach (var extension in extensions)
                {
                    var fullPath = Path.Combine(path, executableName + extension);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
            catch
            {
                // 忽略无效路径
                continue;
            }
        }

        return null;
    }

    /// <summary>
    /// 解析会话路径，支持环境变量查找和直接路径
    /// </summary>
    public static string? ResolveSessionPath(string input)
    {
        // 如果输入包含路径分隔符，认为是直接路径
        if (input.Contains(Path.DirectorySeparatorChar) || input.Contains(Path.AltDirectorySeparatorChar))
        {
            // 展开环境变量
            var expandedPath = Environment.ExpandEnvironmentVariables(input);
            return File.Exists(expandedPath) ? expandedPath : null;
        }

        // 否则在PATH中查找
        return FindExecutableInPath(input);
    }
}

/// <summary>
/// 会话事件参数
/// </summary>
public class SessionEventArgs : EventArgs
{
    public TerminalSession Session { get; }

    public SessionEventArgs(TerminalSession session)
    {
        Session = session;
    }
}
