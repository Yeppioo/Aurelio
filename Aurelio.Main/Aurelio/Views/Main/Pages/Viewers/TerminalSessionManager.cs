using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aurelio.Views.Main.Pages.Viewers;

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
    /// 获取预定义的会话类型
    /// </summary>
    public static Dictionary<string, string> GetPredefinedSessions()
    {
        var sessions = new Dictionary<string, string>();

        // Windows 系统
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            // PowerShell
            var powershellPaths = new[]
            {
                @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                @"C:\Program Files\PowerShell\7\pwsh.exe"
            };
            
            foreach (var path in powershellPaths)
            {
                if (File.Exists(path))
                {
                    sessions["powershell"] = path;
                    break;
                }
            }

            // CMD
            var cmdPath = @"C:\Windows\System32\cmd.exe";
            if (File.Exists(cmdPath))
            {
                sessions["cmd"] = cmdPath;
            }

            // Node.js
            var nodePaths = new[]
            {
                @"C:\Program Files\nodejs\node.exe",
                @"C:\Program Files (x86)\nodejs\node.exe"
            };
            
            foreach (var path in nodePaths)
            {
                if (File.Exists(path))
                {
                    sessions["node"] = path;
                    break;
                }
            }

            // Python
            var pythonPaths = new[]
            {
                @"C:\Python\python.exe",
                @"C:\Program Files\Python\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python312\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python311\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python310\python.exe"
            };
            
            foreach (var path in pythonPaths)
            {
                if (File.Exists(path))
                {
                    sessions["python"] = path;
                    break;
                }
            }
        }

        return sessions;
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
