using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Aurelio.Views.Main.Pages.Viewers.Terminal;

/// <summary>
/// 终端会话类，管理单个终端进程
/// </summary>
public class TerminalSession : IDisposable
{
    public string Id { get; }
    public string Name { get; set; }
    public string ExecutablePath { get; }
    public Process? Process { get; private set; }
    public StreamWriter? Input { get; private set; }
    public bool IsActive { get; set; }
    public DateTime CreatedTime { get; }
    public DateTime LastActiveTime { get; set; }
    public SessionStatus Status { get; set; }

    public event EventHandler<string>? OutputReceived;
    public event EventHandler<string>? ErrorReceived;
    public event EventHandler? ProcessExited;

    public TerminalSession(string name, string executablePath)
    {
        Id = Guid.NewGuid().ToString("N")[..8]; // 8位短ID
        Name = name;
        ExecutablePath = executablePath;
        CreatedTime = DateTime.Now;
        LastActiveTime = DateTime.Now;
        Status = SessionStatus.Created;
    }

    public async Task<bool> StartAsync()
    {
        try
        {
            if (!File.Exists(ExecutablePath))
            {
                Status = SessionStatus.Error;
                return false;
            }

            // 注册编码提供程序
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 获取编码
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding("GBK");
            }
            catch
            {
                try
                {
                    encoding = Encoding.GetEncoding(936);
                }
                catch
                {
                    encoding = Encoding.Default;
                }
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = encoding,
                StandardErrorEncoding = encoding,
                StandardInputEncoding = encoding
            };

            // 设置环境变量
            startInfo.EnvironmentVariables["LANG"] = "zh_CN.GBK";
            startInfo.EnvironmentVariables["LC_ALL"] = "zh_CN.GBK";

            Process = Process.Start(startInfo);
            if (Process == null)
            {
                Status = SessionStatus.Error;
                return false;
            }

            Input = Process.StandardInput;
            Status = SessionStatus.Running;

            // 启动输出监听
            _ = Task.Run(async () => await MonitorOutput());
            _ = Task.Run(async () => await MonitorError());

            // 监听进程退出
            Process.EnableRaisingEvents = true;
            Process.Exited += OnProcessExited;

            return true;
        }
        catch (Exception)
        {
            Status = SessionStatus.Error;
            return false;
        }
    }

    public async Task SendCommandAsync(string command)
    {
        if (Input != null && Process != null && !Process.HasExited)
        {
            await Input.WriteLineAsync(command);
            await Input.FlushAsync();
            LastActiveTime = DateTime.Now;
        }
    }

    public async Task SendInterruptAsync()
    {
        if (Input != null && Process != null && !Process.HasExited)
        {
            await Input.WriteAsync("\x03"); // Ctrl+C
            await Input.FlushAsync();
        }
    }

    public void Stop()
    {
        try
        {
            if (Process != null && !Process.HasExited)
            {
                // 尝试优雅关闭
                try
                {
                    Input?.WriteLine("exit");
                    Input?.Flush();
                    
                    if (!Process.WaitForExit(3000))
                    {
                        Process.Kill(true);
                    }
                }
                catch
                {
                    Process.Kill(true);
                }
            }
            Status = SessionStatus.Stopped;
        }
        catch
        {
            Status = SessionStatus.Error;
        }
    }

    private async Task MonitorOutput()
    {
        if (Process?.StandardOutput == null) return;

        try
        {
            var buffer = new char[1024];
            while (!Process.HasExited)
            {
                var bytesRead = await Process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    var output = new string(buffer, 0, bytesRead);
                    OutputReceived?.Invoke(this, output);
                }
            }
        }
        catch
        {
            // 忽略读取错误
        }
    }

    private async Task MonitorError()
    {
        if (Process?.StandardError == null) return;

        try
        {
            var buffer = new char[1024];
            while (!Process.HasExited)
            {
                var bytesRead = await Process.StandardError.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    var error = new string(buffer, 0, bytesRead);
                    ErrorReceived?.Invoke(this, error);
                }
            }
        }
        catch
        {
            // 忽略读取错误
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        Status = SessionStatus.Exited;
        ProcessExited?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Stop();
        Input?.Dispose();
        Process?.Dispose();
    }
}

/// <summary>
/// 会话状态枚举
/// </summary>
public enum SessionStatus
{
    Created,
    Running,
    Stopped,
    Exited,
    Error
}
