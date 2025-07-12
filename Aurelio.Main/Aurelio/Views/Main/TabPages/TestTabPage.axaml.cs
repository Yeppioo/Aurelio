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

    private TaskEntry? task;

    public DebugTabPage()
    {
        InitializeComponent();
        DataContext = this;
        RootElement = Root;
        InAnimator = new PageLoadingAnimator(Root, new Thickness(0, 60, 0, 0), (0, 1));
        PageInfo = new PageInfoEntry
        {
            Icon = Icons.FromMaterial(MaterialIconKind.Bug),
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
}