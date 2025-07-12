using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.ViewModels;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Material.Icons;

namespace Aurelio.Views.Main.TabPages;

public partial class DebugTabPage : PageMixModelBase, IAurelioTabPage
{
    private readonly Random r = new();

    public DebugTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
    }

    public Control RootElement { get; set; }
    public PageLoadingAnimator InAnimator { get; set; }
    public TabEntry HostTab { get; set; }

    public PageInfoEntry PageInfo { get; } = new()
    {
        Icon = Icon.FromMaterial(MaterialIconKind.Bug),
        Title = "Debug"
    };

    public void OnClose()
    {
    }

    private void BobbleNotice_OnClick(object? sender, RoutedEventArgs e)
    {
        NotificationBubble("Test Message", (NotificationType)r.Next(0, 4), TimeSpan.FromHours(1));
    }

    private void CreateTask_OnClick(object? sender, RoutedEventArgs e)
    {
        var task = Tasking.CreateTask("Test Task");
        var sub = new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished };
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Error });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Canceled });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Canceling });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Running });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Paused });
        sub.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Error });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Canceled });
        var sub2 = new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished };
        sub2.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished });
        sub2.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Error });
        task.SubTasks.Add(sub2);
        var sub1 = new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished };
        sub1.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished });
        sub1.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Error });
        sub2.SubTasks.Add(sub1);
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Finished });
        task.SubTasks.Add(sub2);
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Waiting });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Running });
        task.SubTasks.Add(new TaskEntry { Name = "SubTask", TaskState = TaskState.Paused });
    }
}