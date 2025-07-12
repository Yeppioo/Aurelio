using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Enum;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class TaskEntry : ReactiveObject
{
    public TaskEntry(string name)
    {
        Name = name;
    }

    public TaskEntry()
    {
    }

    [Reactive] public string Name { get; set; }
    [Reactive] public TaskState TaskState { get; set; } = TaskState.Waiting;
    [Reactive] public bool ProgressIsIndeterminate { get; set; } = true;
    [Reactive] public bool IsCancelRequest { get; set; }
    [Reactive] public bool IsButtonEnable { get; set; }
    [Reactive] public bool CanRemove { get; set; }
    [Reactive] public bool Expanded { get; set; } = true;
    [Reactive] public double ProgressValue { get; set; }
    public ObservableCollection<TaskEntry> SubTasks { get; set; } = [];


    public TaskEntry AddIn(TaskEntry entry)
    {
        entry.SubTasks.Add(this);
        return this;
    }

    public void NextSubTask()
    {
        var item = SubTasks.FirstOrDefault(x =>
            x.TaskState == TaskState.Waiting);
        (item ?? SubTasks[0]).TaskState = TaskState.Running;
        if (item == null) return;
        var index = SubTasks.IndexOf(item);
        if (index != 0) SubTasks[index - 1].TaskState = TaskState.Finished;
    }

    public void Destroy()
    {
        Tasking.Tasks.Remove(this);
    }

    public void FinishWithSuccess()
    {
        CanRemove = true;
        IsButtonEnable = true;
        TaskState = TaskState.Finished;
        foreach (var task in SubTasks) task.FinishWithSuccess();
    }

    public void FinishWithError()
    {
        CanRemove = true;
        TaskState = TaskState.Error;
    }

    public void Cancel()
    {
        TaskState = TaskState.Canceled;
        CanRemove = true;
    }

    public void CancelWaitFinish()
    {
        TaskState = TaskState.Canceling;
        IsButtonEnable = false;
    }

    public void CancelWithSuccess()
    {
        TaskState = TaskState.Canceling;
        IsButtonEnable = false;
        FinishWithSuccess();
    }

    public void CancelFinish()
    {
        TaskState = TaskState.Canceled;
        IsButtonEnable = true;
        CanRemove = true;
    }
}