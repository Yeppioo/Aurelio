using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;

namespace Aurelio.Views.Main.Pages.Viewers;

public partial class TerminalViewer : PageMixModelBase, IAurelioTabPage
{
    private Process? _process;
    private StreamWriter? _processInput;
    private readonly StringBuilder _outputBuffer = new();
    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;
    private string _terminalPath;
    private string _terminalTitle = "Terminal";
    private string _processStatus = "未连接";
    private string _currentInput = "";

    public string TerminalTitle
    {
        get => _terminalTitle;
        set => SetField(ref _terminalTitle, value);
    }

    public string ProcessStatus
    {
        get => _processStatus;
        set => SetField(ref _processStatus, value);
    }

    public string CurrentInput
    {
        get => _currentInput;
        set => SetField(ref _currentInput, value);
    }

    public TerminalViewer(string path)
    {
        _terminalPath = path;
        InitializeComponent();
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));

        PageInfo = new PageInfoEntry
        {
            Title = $"Terminal - {Path.GetFileNameWithoutExtension(path)}",
            Icon = StreamGeometry.Parse(
                "M73.4 182.6C60.9 170.1 60.9 149.8 73.4 137.3C85.9 124.8 106.2 124.8 118.7 137.3L278.7 297.3C291.2 309.8 291.2 330.1 278.7 342.6L118.7 502.6C106.2 515.1 85.9 515.1 73.4 502.6C60.9 490.1 60.9 469.8 73.4 457.3L210.7 320L73.4 182.6zM288 448L544 448C561.7 448 576 462.3 576 480C576 497.7 561.7 512 544 512L288 512C270.3 512 256 497.7 256 480C256 462.3 270.3 448 288 448z")
        };

        TerminalTitle = $"Terminal - {Path.GetFileNameWithoutExtension(path)}";
        DataContext = this;

        InitializeTerminal();
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }
    public PageInfoEntry PageInfo { get; }

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

            // 启动终端进程
            StartTerminalProcess();
        }
        catch (Exception ex)
        {
            AppendOutput($"初始化终端失败: {ex.Message}\n", true);
        }
    }

    private void SetupGlobalKeyBindings()
    {
        try
        {
            // 为整个控件设置键盘事件处理
            this.KeyDown += OnGlobalKeyDown;

            // 确保控件可以接收焦点和键盘事件
            this.Focusable = true;
            this.IsTabStop = true;
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

            // Ctrl+D - 关闭终端进程
            if (e.Key == Key.D && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                await CloseTerminalProcess();
                e.Handled = true;
                return;
            }

            // Ctrl+R - 重启终端进程
            if (e.Key == Key.R && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                await RestartTerminalProcess();
                e.Handled = true;
                return;
            }

            // F5 - 重启终端进程（备用快捷键）
            if (e.Key == Key.F5)
            {
                await RestartTerminalProcess();
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

    private void StartTerminalProcess()
    {
        try
        {
            if (!File.Exists(_terminalPath))
            {
                AppendOutput($"错误: 找不到终端程序 '{_terminalPath}'\n", true);
                ProcessStatus = "错误";
                return;
            }

            // 注册编码提供程序以支持GBK等编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 尝试获取GBK编码，如果失败则使用系统默认编码
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding("GBK");
            }
            catch
            {
                try
                {
                    // 尝试使用936代码页（简体中文GBK）
                    encoding = Encoding.GetEncoding(936);
                }
                catch
                {
                    // 如果都失败，使用系统默认编码
                    encoding = Encoding.Default;
                }
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _terminalPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = encoding,
                StandardErrorEncoding = encoding,
                StandardInputEncoding = encoding
            };

            // 设置环境变量以支持中文GBK编码
            startInfo.EnvironmentVariables["LANG"] = "zh_CN.GBK";
            startInfo.EnvironmentVariables["LC_ALL"] = "zh_CN.GBK";

            _process = Process.Start(startInfo);

            if (_process == null)
            {
                AppendOutput("错误: 无法启动终端进程\n", true);
                ProcessStatus = "错误";
                return;
            }

            _processInput = _process.StandardInput;
            ProcessStatus = "已连接";

            // 启动输出监听
            Task.Run(MonitorOutput);
            Task.Run(MonitorError);

            // 监听进程退出
            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessExited;

            AppendOutput($"终端已启动: {Path.GetFileName(_terminalPath)}\n");
            AppendOutput($"使用编码: {encoding.EncodingName} (代码页: {encoding.CodePage})\n");
        }
        catch (Exception ex)
        {
            AppendOutput($"启动终端失败: {ex.Message}\n", true);
            ProcessStatus = "错误";
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
        if (_process == null || _processInput == null || _process.HasExited)
        {
            AppendOutput("错误: 终端进程未运行\n", true);
            return;
        }

        try
        {
            // 添加到命令历史
            if (!string.IsNullOrWhiteSpace(command))
            {
                _commandHistory.Add(command);
                _historyIndex = _commandHistory.Count;
            }

            // 显示用户输入
            AppendOutput($"> {command}\n");

            // 发送命令到进程
            await _processInput.WriteLineAsync(command);
            await _processInput.FlushAsync();

            // 清空输入框
            CurrentInput = "";
        }
        catch (Exception ex)
        {
            AppendOutput($"发送命令失败: {ex.Message}\n", true);
        }
    }

    private async Task SendInterrupt()
    {
        if (_process == null || _process.HasExited)
            return;

        try
        {
            // 发送 Ctrl+C 信号
            await _processInput?.WriteAsync("\x03");
            await _processInput?.FlushAsync();
            AppendOutput("^C\n");
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

    private async Task MonitorOutput()
    {
        if (_process?.StandardOutput == null) return;

        try
        {
            var buffer = new char[1024];
            while (!_process.HasExited)
            {
                var bytesRead = await _process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    var output = new string(buffer, 0, bytesRead);
                    await Dispatcher.UIThread.InvokeAsync(() => AppendOutput(output));
                }
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                AppendOutput($"输出监听错误: {ex.Message}\n", true));
        }
    }

    private async Task MonitorError()
    {
        if (_process?.StandardError == null) return;

        try
        {
            var buffer = new char[1024];
            while (!_process.HasExited)
            {
                var bytesRead = await _process.StandardError.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    var error = new string(buffer, 0, bytesRead);
                    await Dispatcher.UIThread.InvokeAsync(() => AppendOutput(error, true));
                }
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                AppendOutput($"错误监听失败: {ex.Message}\n", true));
        }
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

    private async Task CloseTerminalProcess()
    {
        try
        {
            if (_process != null && !_process.HasExited)
            {
                AppendOutput("\n正在关闭终端进程...\n");

                // 尝试优雅关闭
                try
                {
                    await _processInput?.WriteLineAsync("exit");
                    await _processInput?.FlushAsync();

                    // 等待进程退出
                    if (!_process.WaitForExit(3000))
                    {
                        _process.Kill(true);
                        AppendOutput("进程已强制终止\n");
                    }
                    else
                    {
                        AppendOutput("进程已正常退出\n");
                    }
                }
                catch
                {
                    _process.Kill(true);
                    AppendOutput("进程已强制终止\n");
                }

                ProcessStatus = "已断开";
            }
            else
            {
                AppendOutput("没有运行中的进程\n");
            }
        }
        catch (Exception ex)
        {
            AppendOutput($"关闭进程失败: {ex.Message}\n", true);
        }
    }

    private async Task RestartTerminalProcess()
    {
        try
        {
            AppendOutput("\n正在重启终端...\n");

            // 关闭当前进程
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _process.Kill(true);
                }
                catch
                {
                }
            }

            // 清理资源
            CleanupProcess();

            // 等待一小段时间
            await Task.Delay(500);

            // 重新启动
            StartTerminalProcess();
        }
        catch (Exception ex)
        {
            AppendOutput($"重启终端失败: {ex.Message}\n", true);
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

    #endregion

    private void OnProcessExited(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProcessStatus = "已断开";
            AppendOutput("\n进程已退出\n", true);
        });
    }

    public void OnClose()
    {
        _ = Task.Run(CleanupProcess);
    }

    private void CleanupProcess()
    {
        try
        {
            if (_process != null && !_process.HasExited)
            {
                // 尝试优雅地关闭进程
                _processInput?.WriteLine("exit");
                _processInput?.Flush();

                // 等待一段时间让进程自然退出
                if (!_process.WaitForExit(3000))
                {
                    // 如果进程没有退出，强制终止
                    _process.Kill(true);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"清理进程时出错: {ex.Message}");
        }
        finally
        {
            _processInput?.Dispose();
            _process?.Dispose();
            _processInput = null;
            _process = null;
        }
    }
}