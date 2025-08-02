using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.Viewers;
using Aurelio.Views.Main.Pages.Viewers.Terminal;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Aurelio.Views.Main.Pages;

public partial class DebugTabPage : PageMixModelBase, IAurelioTabPage
{
    private readonly Random r = new();

    private TaskEntry? task;

    public DebugTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Icon = StreamGeometry.Parse(
                "M256 0c53 0 96 43 96 96l0 3.6c0 15.7-12.7 28.4-28.4 28.4l-135.1 0c-15.7 0-28.4-12.7-28.4-28.4l0-3.6c0-53 43-96 96-96zM41.4 105.4c12.5-12.5 32.8-12.5 45.3 0l64 64c.7 .7 1.3 1.4 1.9 2.1c14.2-7.3 30.4-11.4 47.5-11.4l112 0c17.1 0 33.2 4.1 47.5 11.4c.6-.7 1.2-1.4 1.9-2.1l64-64c12.5-12.5 32.8-12.5 45.3 0s12.5 32.8 0 45.3l-64 64c-.7 .7-1.4 1.3-2.1 1.9c6.2 12 10.1 25.3 11.1 39.5l64.3 0c17.7 0 32 14.3 32 32s-14.3 32-32 32l-64 0c0 24.6-5.5 47.8-15.4 68.6c2.2 1.3 4.2 2.9 6 4.8l64 64c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0l-63.1-63.1c-24.5 21.8-55.8 36.2-90.3 39.6L272 240c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 239.2c-34.5-3.4-65.8-17.8-90.3-39.6L86.6 502.6c-12.5 12.5-32.8 12.5-45.3 0s-12.5-32.8 0-45.3l64-64c1.9-1.9 3.9-3.4 6-4.8C101.5 367.8 96 344.6 96 320l-64 0c-17.7 0-32-14.3-32-32s14.3-32 32-32l64.3 0c1.1-14.1 5-27.5 11.1-39.5c-.7-.6-1.4-1.2-2.1-1.9l-64-64c-12.5-12.5-12.5-32.8 0-45.3z"),
            Title = "Debug"
        };
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; }

    public void OnClose()
    {
    }

    private void BobbleNotice_OnClick(object? sender, RoutedEventArgs e)
    {
        NotificationBubble("Test Message", (NotificationType)r.Next(0, 4), TimeSpan.FromHours(1));
    }

    private void Crash_OnClick(object? sender, RoutedEventArgs e)
    {
        var a = 0;
        var b = 0 / a;
    }

    private void NextTask_OnClick(object? sender, RoutedEventArgs e)
    {
        task.NextSubTask();
    }

    private void CreateTask_OnClick(object? sender, RoutedEventArgs e)
    {
        task = Tasking.CreateTask("Test Task");
        task.ProgressIsIndeterminate = false;
        task.ProgressValue = 100;
        task.IsButtonEnable = true;
        task.ButtonText = "ButtonText";
        task.ButtonAction = () => { task.Cancel(); };
        task.BottomLeftInfoText = "BottomLeftInfoText";
        task.TopRightInfoText = "TopRightInfoText";
        task.IsDestroyButtonVisible = true;

        var sub = new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting };
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        var sub2 = new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting };
        sub2.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub2.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        task.SubTasks.Add(sub2);
        var sub1 = new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting };
        sub1.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub1.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub2.SubTasks.Add(sub1);
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
    }

    private void Console_OnClick(object? sender, RoutedEventArgs e)
    {
        // Create a PowerShell terminal
        // var terminal = new TerminalViewer(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe");
        var terminal = new TerminalViewer(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe");
        // var terminal = new TerminalViewer(@"C:\Program Files\nodejs\node.exe");
        App.UiRoot.TogglePage(null, terminal);
    }
}