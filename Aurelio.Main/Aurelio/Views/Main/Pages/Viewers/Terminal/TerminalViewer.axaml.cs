using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Langs;
using Aurelio.Public.Module;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;

namespace Aurelio.Views.Main.Pages.Viewers.Terminal;

public partial class TerminalViewer : PageMixModelBase, IAurelioTabPage, IAurelioNavPage
{
    private readonly TerminalSessionManager _sessionManager = new();
    private readonly StringBuilder _outputBuffer = new();
    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;
    private string _initialTerminalPath;
    private string _currentInput = "";
    private readonly Dictionary<string, string> _builtInCommands;

    public string CurrentInput
    {
        get => _currentInput;
        set => SetField(ref _currentInput, value);
    }
    
    private string _shortInfo = string.Empty;

    public string ShortInfo
    {
        get => _shortInfo;
        set => SetField(ref _shortInfo, value);
    }

    public Control BottomElement { get; set; }


    public TerminalViewer()
    {
    }

    public TerminalViewer(string path)
    {
        _initialTerminalPath = path;
        _builtInCommands = InitializeBuiltInCommands();

        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));

        PageInfo = new PageInfoEntry
        {
            Title = $"{MainLang.Terminal} - {Path.GetFileNameWithoutExtension(path)}",
            Icon = StreamGeometry.Parse(
                "M73.4 182.6C60.9 170.1 60.9 149.8 73.4 137.3C85.9 124.8 106.2 124.8 118.7 137.3L278.7 297.3C291.2 309.8 291.2 330.1 278.7 342.6L118.7 502.6C106.2 515.1 85.9 515.1 73.4 502.6C60.9 490.1 60.9 469.8 73.4 457.3L210.7 320L73.4 182.6zM288 448L544 448C561.7 448 576 462.3 576 480C576 497.7 561.7 512 544 512L288 512C270.3 512 256 497.7 256 480C256 462.3 270.3 448 288 448z")
        };
        
        ShortInfo = $"{MainLang.Terminal} / {Path.GetFileNameWithoutExtension(path)}";

        DataContext = this;

        InitializeTerminal();
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

    private Dictionary<string, string> InitializeBuiltInCommands()
    {
        return new Dictionary<string, string>
        {
            ["sessions"] = "列出所有会话",
            ["session"] = "切换到指定会话 (用法: session <id>)",
            ["new"] = "创建新会话 (用法: new <程序名|路径> [名称])",
            ["stop"] = "停止指定会话 (用法: stop <id>)",
            ["help"] = "显示帮助信息",
            ["clear"] = "清空输出",
            ["debug"] = "显示调试信息",
            ["exit"] = "退出终端",
            ["quit"] = "退出终端"
        };
    }

    private void InitializeTerminal()
    {
        try
        {
            // 设置输入框事件处理
            InputTextBox.KeyDown += OnInputKeyDown;

            // 初始化输出编辑器
            OutputEditor.Document = new TextDocument();
            OutputEditor.FontFamily = new FontFamily("Consolas, 'Courier New', monospace");

            // 设置全局快捷键
            SetupGlobalKeyBindings();

            // 设置会话管理事件
            SetupSessionManager();

            // 创建初始会话
            CreateInitialSession();
        }
        catch (Exception ex)
        {
            AppendOutput($"初始化终端失败: {ex.Message}\n", true);
        }
    }

    private void SetupSessionManager()
    {
        try
        {
            _sessionManager.SessionCreated += OnSessionCreated;
            _sessionManager.SessionStopped += OnSessionStopped;
            _sessionManager.SessionActivated += OnSessionActivated;
        }
        catch (Exception ex)
        {
            AppendOutput($"设置会话管理器失败: {ex.Message}\n", true);
        }
    }

    private void CreateInitialSession()
    {
        try
        {
            // 先解析路径，与命令创建会话保持一致的逻辑
            var executablePath = TerminalSessionManager.ResolveSessionPath(_initialTerminalPath);

            if (executablePath != null)
            {
                var sessionName = Path.GetFileNameWithoutExtension(_initialTerminalPath);
                var session = _sessionManager.CreateSession(sessionName, executablePath);

                if (session != null)
                {
                    // 设置会话事件处理
                    session.OutputReceived += OnSessionOutput;
                    session.ErrorReceived += OnSessionError;
                    session.ProcessExited += OnSessionExited;

                    AppendOutput($"会话已创建: {session.Name} (ID: {session.Id})\n");
                    AppendOutput($"可执行文件: {executablePath}\n");
                    AppendOutput($"使用编码: GBK\n");
                    AppendOutput("输入 'help' 查看内置命令\n\n");
                }
                else
                {
                    AppendOutput($"创建初始会话失败: {executablePath}\n", true);
                }
            }
            else
            {
                AppendOutput($"找不到可执行文件: {_initialTerminalPath}\n", true);
                AppendOutput("请检查路径是否正确或程序是否已安装\n", true);
            }
        }
        catch (Exception ex)
        {
            AppendOutput($"初始化会话失败: {ex.Message}\n", true);
        }
    }

    private void OnSessionCreated(object? sender, SessionEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            AppendOutput($"新会话已创建: {e.Session.Name} (ID: {e.Session.Id})\n");
            UpdateTerminalTitle();
        });
    }

    private void OnSessionStopped(object? sender, SessionEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            AppendOutput($"会话已停止: {e.Session.Name} (ID: {e.Session.Id})\n");
            UpdateTerminalTitle();
        });
    }

    private void OnSessionActivated(object? sender, SessionEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            AppendOutput($"切换到会话: {e.Session.Name} (ID: {e.Session.Id})\n");
            UpdateTerminalTitle();
        });
    }

    private void OnSessionOutput(object? sender, string output)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (sender is TerminalSession session)
            {
                var processedOutput = ProcessSessionOutput(session, output);
                AppendOutput(processedOutput);
            }
            else
            {
                AppendOutput(output);
            }
        });
    }

    private void OnSessionError(object? sender, string error)
    {
        Dispatcher.UIThread.InvokeAsync(() => AppendOutput(error, true));
    }

    private void OnSessionExited(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (sender is TerminalSession session)
            {
                AppendOutput($"\n会话 {session.Name} (ID: {session.Id}) 已退出\n");
                UpdateTerminalTitle();
            }
        });
    }

    private void UpdateTerminalTitle()
    {
        var activeSession = _sessionManager.ActiveSession;
        if (activeSession != null)
        {
            // 更新PageInfo的Title
            PageInfo.Title = $"{MainLang.Terminal} - {activeSession.Name}";
            ShortInfo = $"{MainLang.Terminal} - {activeSession.Name}";
        }
        else
        {
            // 更新PageInfo的Title
            PageInfo.Title = $"{MainLang.Terminal} - 无活动会话";
            ShortInfo = $"{MainLang.Terminal} - 无活动会话";
        }
    }

    private void SetupGlobalKeyBindings()
    {
        try
        {
            // 为整个控件设置键盘事件处理
            KeyDown += OnGlobalKeyDown;

            // 确保控件可以接收焦点和键盘事件
            Focusable = true;
            IsTabStop = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"设置快捷键失败: {ex.Message}");
        }
    }

    private async void OnGlobalKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            // Ctrl+L - 清空终端输出
            if (e.Key == Key.L && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                ClearTerminalOutput();
                e.Handled = true;
                return;
            }

            // Ctrl+K - 清空终端输出（备用快捷键）
            if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                ClearTerminalOutput();
                e.Handled = true;
                return;
            }

            // Ctrl+D - 关闭当前会话
            if (e.Key == Key.D && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                HandleBuiltInCommand("stop " + (_sessionManager.ActiveSession?.Id ?? ""));
                e.Handled = true;
                return;
            }

            // Ctrl+R - 重启当前会话
            if (e.Key == Key.R && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                await RestartCurrentSession();
                e.Handled = true;
                return;
            }

            // F5 - 重启当前会话（备用快捷键）
            if (e.Key == Key.F5)
            {
                await RestartCurrentSession();
                e.Handled = true;
                return;
            }

            // Ctrl+Shift+C - 复制选中的文本
            if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control | KeyModifiers.Shift))
            {
                CopySelectedText();
                e.Handled = true;
                return;
            }

            // Ctrl+Shift+V - 粘贴文本到输入框
            if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control | KeyModifiers.Shift))
            {
                await PasteTextToInput();
                e.Handled = true;
                return;
            }

            // Escape - 取消当前输入
            if (e.Key == Key.Escape)
            {
                CancelCurrentInput();
                e.Handled = true;
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"处理快捷键时出错: {ex.Message}");
        }
    }


    private async void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await SendCommand(CurrentInput);
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            NavigateHistory(-1);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            NavigateHistory(1);
            e.Handled = true;
        }
        else if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            // Ctrl+C - 中断当前命令
            await SendInterrupt();
            e.Handled = true;
        }
    }

    private async Task SendCommand(string command)
    {
        try
        {
            // 添加到命令历史（只有非空命令）
            if (!string.IsNullOrWhiteSpace(command))
            {
                _commandHistory.Add(command);
                _historyIndex = _commandHistory.Count;
            }

            // 检查是否为内置命令（只有非空命令）
            if (!string.IsNullOrWhiteSpace(command) && HandleBuiltInCommand(command))
            {
                // 内置命令显示用户输入
                AppendOutput($"> {command}\n");
                CurrentInput = "";
                return;
            }

            // 发送到活动会话（包括空命令，对交互式程序很重要）
            var activeSession = _sessionManager.ActiveSession;
            if (activeSession != null)
            {
                // 直接发送命令，不显示额外的提示符
                // 程序本身会处理提示符的显示
                await activeSession.SendCommandAsync(command);
            }
            else
            {
                AppendOutput("错误: 没有活动会话\n", true);
            }

            // 清空输入框
            CurrentInput = "";
        }
        catch (Exception ex)
        {
            AppendOutput($"发送命令失败: {ex.Message}\n", true);
        }
    }

    private bool HandleBuiltInCommand(string command)
    {
        var parts = command.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        var cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "help":
                ShowHelp();
                return true;

            case "sessions":
                ShowSessions();
                return true;

            case "session":
                if (parts.Length > 1)
                {
                    SwitchSession(parts[1]);
                }
                else
                {
                    AppendOutput("用法: session <id>\n", true);
                }

                return true;

            case "new":
                if (parts.Length > 1)
                {
                    var sessionType = parts[1];
                    var sessionName = parts.Length > 2 ? parts[2] : sessionType;
                    CreateNewSession(sessionType, sessionName);
                }
                else
                {
                    AppendOutput("用法: new <type> [name]\n", true);
                    ShowAvailableSessionTypes();
                }

                return true;

            case "stop":
                if (parts.Length > 1)
                {
                    StopSession(parts[1]);
                }
                else
                {
                    AppendOutput("用法: stop <id>\n", true);
                }

                return true;

            case "clear":
                ClearTerminalOutput();
                return true;

            case "exit":
                OnClose();
                return true;

            case "quit":
                OnClose();
                return true;

            case "debug":
                ShowDebugInfo();
                return true;

            default:
                return false; // 不是内置命令
        }
    }

    private void ShowHelp()
    {
        AppendOutput("=== 内置命令帮助 ===\n");
        foreach (var cmd in _builtInCommands)
        {
            AppendOutput($"  {cmd.Key,-12} - {cmd.Value}\n");
        }

        AppendOutput("\n");
        ShowAvailableSessionTypes();
        AppendOutput("\n");
    }

    private void ShowSessions()
    {
        AppendOutput("=== 当前会话列表 ===\n");
        var sessions = _sessionManager.AllSessions.ToList();

        if (!sessions.Any())
        {
            AppendOutput("  没有活动会话\n");
        }
        else
        {
            foreach (var session in sessions)
            {
                var status = session.IsActive ? "[活动]" : "";
                var uptime = DateTime.Now - session.CreatedTime;
                AppendOutput(
                    $"  {session.Id} - {session.Name} ({session.Status}) {status} 运行时间: {uptime:hh\\:mm\\:ss}\n");
            }
        }

        AppendOutput("\n");
    }

    private void SwitchSession(string sessionId)
    {
        if (_sessionManager.SwitchToSession(sessionId))
        {
            AppendOutput($"已切换到会话: {sessionId}\n");
        }
        else
        {
            AppendOutput($"会话不存在: {sessionId}\n", true);
        }
    }

    private void CreateNewSession(string sessionType, string sessionName)
    {
        // 尝试解析会话路径
        var executablePath = TerminalSessionManager.ResolveSessionPath(sessionType);

        if (executablePath != null)
        {
            var session = _sessionManager.CreateSession(sessionName, executablePath);
            if (session != null)
            {
                // 设置会话事件处理
                session.OutputReceived += OnSessionOutput;
                session.ErrorReceived += OnSessionError;
                session.ProcessExited += OnSessionExited;

                AppendOutput($"新会话已创建: {sessionName} (ID: {session.Id})\n");
                AppendOutput($"可执行文件: {executablePath}\n");

                // 自动切换到新会话
                _sessionManager.SwitchToSession(session.Id);
            }
            else
            {
                AppendOutput($"创建会话失败: {executablePath}\n", true);
            }
        }
        else
        {
            AppendOutput($"找不到可执行文件: {sessionType}\n", true);
            AppendOutput("请使用以下格式之一:\n");
            AppendOutput("  new <程序名> [会话名]     - 在PATH中查找程序\n");
            AppendOutput("  new <完整路径> [会话名]   - 使用完整路径\n");
            AppendOutput("  new %PROGRAMFILES%\\... [会话名] - 使用环境变量\n");
            AppendOutput("\n示例:\n");
            AppendOutput("  new powershell\n");
            AppendOutput("  new node\n");
            AppendOutput("  new python\n");
            AppendOutput("  new cmd\n");
            AppendOutput("  new C:\\Windows\\System32\\cmd.exe\n");
            AppendOutput("  new %PROGRAMFILES%\\Git\\bin\\bash.exe\n");
        }
    }

    private void StopSession(string sessionId)
    {
        if (_sessionManager.StopSession(sessionId))
        {
            AppendOutput($"会话已停止: {sessionId}\n");
        }
        else
        {
            AppendOutput($"会话不存在: {sessionId}\n", true);
        }
    }

    private void ShowAvailableSessionTypes()
    {
        AppendOutput("=== 会话创建方式 ===\n");
        AppendOutput("1. 程序名 (在PATH中查找):\n");
        AppendOutput("   new powershell\n");
        AppendOutput("   new cmd\n");
        AppendOutput("   new node\n");
        AppendOutput("   new python\n");
        AppendOutput("   new git-bash\n");
        AppendOutput("\n2. 完整路径:\n");
        AppendOutput("   new C:\\Windows\\System32\\cmd.exe\n");
        AppendOutput("   new C:\\Program Files\\Git\\bin\\bash.exe\n");
        AppendOutput("\n3. 环境变量路径:\n");
        AppendOutput("   new %PROGRAMFILES%\\Git\\bin\\bash.exe\n");
        AppendOutput("   new %USERPROFILE%\\AppData\\Local\\Programs\\Python\\Python312\\python.exe\n");
        AppendOutput("\n4. 带自定义名称:\n");
        AppendOutput("   new powershell my-ps\n");
        AppendOutput("   new node nodejs-dev\n");
    }

    private async Task RestartCurrentSession()
    {
        try
        {
            var activeSession = _sessionManager.ActiveSession;
            if (activeSession != null)
            {
                AppendOutput($"\n正在重启会话: {activeSession.Name} ({activeSession.Id})\n");

                var sessionName = activeSession.Name;
                var executablePath = activeSession.ExecutablePath;
                var sessionId = activeSession.Id;

                // 停止当前会话
                _sessionManager.StopSession(sessionId);

                // 等待一小段时间
                await Task.Delay(500);

                // 创建新会话
                var newSession = _sessionManager.CreateSession(sessionName, executablePath);
                if (newSession != null)
                {
                    // 设置会话事件处理
                    newSession.OutputReceived += OnSessionOutput;
                    newSession.ErrorReceived += OnSessionError;
                    newSession.ProcessExited += OnSessionExited;

                    AppendOutput($"会话已重启: {newSession.Name} (新ID: {newSession.Id})\n");

                    // 自动切换到新会话
                    _sessionManager.SwitchToSession(newSession.Id);
                }
                else
                {
                    AppendOutput("重启会话失败\n", true);
                }
            }
            else
            {
                AppendOutput("没有活动会话可重启\n", true);
            }
        }
        catch (Exception ex)
        {
            AppendOutput($"重启会话失败: {ex.Message}\n", true);
        }
    }

    private async Task SendInterrupt()
    {
        try
        {
            var activeSession = _sessionManager.ActiveSession;
            if (activeSession != null)
            {
                await activeSession.SendInterruptAsync();
                AppendOutput("^C\n");
            }
            else
            {
                AppendOutput("没有活动会话可中断\n", true);
            }
        }
        catch (Exception ex)
        {
            AppendOutput($"中断命令失败: {ex.Message}\n", true);
        }
    }

    private void NavigateHistory(int direction)
    {
        if (_commandHistory.Count == 0) return;

        _historyIndex += direction;

        if (_historyIndex < 0)
            _historyIndex = 0;
        else if (_historyIndex >= _commandHistory.Count)
        {
            _historyIndex = _commandHistory.Count;
            CurrentInput = "";
            return;
        }

        CurrentInput = _commandHistory[_historyIndex];
    }


    private void AppendOutput(string text, bool isError = false)
    {
        try
        {
            _outputBuffer.Append(text);

            // 限制缓冲区大小，防止内存溢出
            if (_outputBuffer.Length > 100000)
            {
                var excess = _outputBuffer.Length - 80000;
                _outputBuffer.Remove(0, excess);
            }

            OutputEditor.Document.Text = _outputBuffer.ToString();

            // 改进的滚动逻辑：滚动到最后一行，而不是完全底部
            SmartScrollToBottom();
        }
        catch (Exception ex)
        {
            // 避免无限递归，直接输出到调试
            System.Diagnostics.Debug.WriteLine($"AppendOutput error: {ex.Message}");
        }
    }

    private void SmartScrollToBottom()
    {
        try
        {
            // 使用Dispatcher确保在UI线程中执行
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    var document = OutputEditor.Document;
                    if (document.LineCount > 0)
                    {
                        // 滚动到最后一行，这比ScrollToEnd()产生的空白更少
                        OutputEditor.ScrollTo(document.LineCount, 0);
                    }
                }
                catch (Exception innerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SmartScrollToBottom inner error: {innerEx.Message}");
                    // 回退到默认滚动
                    OutputEditor.ScrollToEnd();
                }
            }, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SmartScrollToBottom error: {ex.Message}");
            // 最简单的回退方案
            try
            {
                OutputEditor.ScrollToEnd();
            }
            catch
            {
                // 如果连ScrollToEnd都失败，就忽略滚动
            }
        }
    }

    #region 快捷键功能实现

    private void ClearTerminalOutput()
    {
        try
        {
            _outputBuffer.Clear();
            OutputEditor.Document.Text = "";
            AppendOutput("终端输出已清空\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"清空终端输出失败: {ex.Message}");
        }
    }


    private void CopySelectedText()
    {
        try
        {
            var selectedText = OutputEditor.SelectedText;
            if (!string.IsNullOrEmpty(selectedText))
            {
                // 复制到剪贴板
                TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(selectedText);
                AppendOutput($"已复制 {selectedText.Length} 个字符到剪贴板\n");
            }
            else
            {
                AppendOutput("没有选中的文本\n");
            }
        }
        catch (Exception ex)
        {
            AppendOutput($"复制文本失败: {ex.Message}\n", true);
        }
    }

    private async Task PasteTextToInput()
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                var text = await clipboard.GetTextAsync();
                if (!string.IsNullOrEmpty(text))
                {
                    // 将文本添加到当前输入
                    CurrentInput += text;
                    InputTextBox.Focus();
                }
            }
        }
        catch (Exception ex)
        {
            AppendOutput($"粘贴文本失败: {ex.Message}\n", true);
        }
    }

    private void CancelCurrentInput()
    {
        try
        {
            CurrentInput = "";
            InputTextBox.Focus();
            AppendOutput("已取消当前输入\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"取消输入失败: {ex.Message}");
        }
    }

    private string ProcessSessionOutput(TerminalSession session, string output)
    {
        // 暂时简化处理，只做基本的清理
        return output;
    }


    private void ShowDebugInfo()
    {
        AppendOutput("=== 调试信息 ===\n");
        var activeSession = _sessionManager.ActiveSession;
        if (activeSession != null)
        {
            AppendOutput($"活动会话: {activeSession.Name} (ID: {activeSession.Id})\n");
            AppendOutput($"可执行文件: {activeSession.ExecutablePath}\n");
            AppendOutput($"进程状态: {activeSession.Status}\n");
            AppendOutput($"进程ID: {activeSession.Process?.Id ?? -1}\n");
            AppendOutput($"进程已退出: {activeSession.Process?.HasExited ?? true}\n");
            AppendOutput($"创建时间: {activeSession.CreatedTime:yyyy-MM-dd HH:mm:ss}\n");
            AppendOutput($"最后活动: {activeSession.LastActiveTime:yyyy-MM-dd HH:mm:ss}\n");

            // 检查进程流状态
            if (activeSession.Process != null)
            {
                try
                {
                    AppendOutput($"标准输入可用: {activeSession.Process.StandardInput != null}\n");
                    AppendOutput($"标准输出可用: {activeSession.Process.StandardOutput != null}\n");
                    AppendOutput($"标准错误可用: {activeSession.Process.StandardError != null}\n");
                }
                catch (Exception ex)
                {
                    AppendOutput($"检查流状态时出错: {ex.Message}\n");
                }
            }
        }
        else
        {
            AppendOutput("没有活动会话\n");
        }

        AppendOutput($"总会话数: {_sessionManager.AllSessions.Count()}\n");
        AppendOutput("===============\n");
    }

    #endregion

    public void OnClose()
    {
        CleanupAllSessions();
    }

    private void CleanupAllSessions()
    {
        try
        {
            _sessionManager.CleanupAllSessions();
            AppendOutput("所有会话已清理\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"清理会话时出错: {ex.Message}");
        }
    }

    public static IAurelioNavPage Create((object sender, object? param) t)
    {
        if (!((string)t.param!).IsNullOrWhiteSpace())
            return new TerminalViewer((string)t.param!);

        // 根据操作系统选择默认终端
        string defaultTerminal;

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            // Windows系统：Win10+使用PowerShell，Win7使用cmd
            if (Environment.OSVersion.Version.Major >= 10)
            {
                defaultTerminal = "powershell";
            }
            else
            {
                defaultTerminal = "cmd";
            }
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            // Linux系统使用bash
            defaultTerminal = "/bin/bash";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
        {
            // macOS系统使用zsh（macOS Catalina 10.15+的默认shell）
            defaultTerminal = "/bin/zsh";
        }
        else
        {
            // 其他系统回退到bash
            defaultTerminal = "/bin/bash";
        }

        return new TerminalViewer(defaultTerminal);
    }

    public static AurelioStaticPageInfo StaticPageInfo { get; } = new()
    {
        Icon = StreamGeometry.Parse(
            "M73.4 182.6C60.9 170.1 60.9 149.8 73.4 137.3C85.9 124.8 106.2 124.8 118.7 137.3L278.7 297.3C291.2 309.8 291.2 330.1 278.7 342.6L118.7 502.6C106.2 515.1 85.9 515.1 73.4 502.6C60.9 490.1 60.9 469.8 73.4 457.3L210.7 320L73.4 182.6zM288 448L544 448C561.7 448 576 462.3 576 480C576 497.7 561.7 512 544 512L288 512C270.3 512 256 497.7 256 480C256 462.3 270.3 448 288 448z"),
        Title = "终端",
        NeedPath = true,
        AutoCreate = false,
        MustPath = false,
    };
}