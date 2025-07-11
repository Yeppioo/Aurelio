using System.Threading.Tasks;
using Aurelio.Public.Classes.Enum;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class TaskEntry : ReactiveObject
{
    [Reactive] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StreamGeometry? Icon { get; set; }
    public TaskState TaskState { get; set; } = TaskState.Waiting;

    public TaskEntry CreateTask(string name)
    {
        var task = new TaskEntry
        {
            Title = name,
            TaskState = TaskState.Waiting
        };
        Tasking.Instance.Tasks.Add(task);
        return task;
    }
}