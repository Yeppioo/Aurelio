using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class TaskEntry : ReactiveObject
{
    public TaskEntry(string name)
    {
        Name = name;
        DestroyAction = Destroy;
        _timer = new Timer(1000);
        _timer.Elapsed += (sender, args) => { Time = Time.Add(TimeSpan.FromSeconds(1)); };
        PropertyChanged += OnPropertyChanged;
        PropertyChanging += (_, E) => { Console.WriteLine($"{E.PropertyName} , {E.ToString()}"); };
    }

    public TaskEntry()
    {
        DestroyAction = Destroy;
        _timer = new Timer(1000);
        _timer.Elapsed += (sender, args) => { Time = Time.Add(TimeSpan.FromSeconds(1)); };
        PropertyChanged += OnPropertyChanged;
    }

    [Reactive] public string Name { get; set; }
    [Reactive] public TaskState TaskState { get; set; } = TaskState.Waiting;
    [Reactive] public bool ProgressIsIndeterminate { get; set; } = true;
    [Reactive] public bool IsCancelRequest { get; set; }
    [Reactive] public bool IsButtonEnable { get; set; } = true;
    [Reactive] public bool Expanded { get; set; } = true;
    [Reactive] public double ProgressValue { get; set; }
    public ObservableCollection<TaskEntry> SubTasks { get; set; } = [];
    public ObservableCollection<OperateButtonEntry> OperateButtons { get; set; } = [];
    [Reactive] public Action? ButtonAction { get; set; }
    [Reactive] public Action? DestroyAction { get; set; } 
    [Reactive] public string ButtonText { get; set; }
    [Reactive] public string TopRightInfoText { get; set; }
    [Reactive] public string BottomLeftInfoText { get; set; }
    [Reactive] public bool IsDestroyButtonVisible { get; set; }
    [Reactive] public TimeSpan Time { get; set; } = TimeSpan.Zero;

    public TaskEntry AddIn(TaskEntry entry)
    {
        entry.SubTasks.Add(this);
        PropertyChanged += OnPropertyChanged;
        return this;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TaskState)) return;
        Tasking.Instance.UpdateDisplay();
        if (TaskState is TaskState.Running or TaskState.Canceling)
        {
            BeginTimer();
        }
        else
        {
            StopTimer();
        }
    }

    private readonly Timer _timer;

    public void BeginTimer()
    {
        _timer.Start();
    }

    public void StopTimer()
    {
        _timer.Stop();
    }

    public void NextSubTask()
    {
        // 如果没有子任务，直接返回
        if (SubTasks.Count == 0) return;

        // 查找当前运行中的子任务
        var runningTask = SubTasks.FirstOrDefault(t => t.TaskState == TaskState.Running);

        if (runningTask != null)
        {
            // 如果运行中的子任务有子任务，先处理子任务
            if (runningTask.SubTasks.Count > 0)
            {
                // 检查运行中子任务的子任务是否都已完成
                var allSubTasksCompleted = runningTask.SubTasks.All(t =>
                    t.TaskState == TaskState.Finished ||
                    t.TaskState == TaskState.Error ||
                    t.TaskState == TaskState.Canceled);

                if (!allSubTasksCompleted)
                {
                    // 如果子任务未全部完成，递归处理子任务
                    runningTask.NextSubTask();
                    return;
                }
            }

            // 将当前运行中的任务标记为完成
            runningTask.FinishWithSuccess();

            // 获取下一个等待中的任务
            int currentIndex = SubTasks.IndexOf(runningTask);
            var nextTask = SubTasks.Skip(currentIndex + 1).FirstOrDefault(t => t.TaskState == TaskState.Waiting);

            if (nextTask != null)
            {
                // 有下一个任务，设置为运行中
                nextTask.TaskState = TaskState.Running;
            }
        }
        else
        {
            // 没有运行中的任务，将第一个等待中的任务设置为运行中
            var firstWaitingTask = SubTasks.FirstOrDefault(t => t.TaskState == TaskState.Waiting);
            if (firstWaitingTask != null)
            {
                firstWaitingTask.TaskState = TaskState.Running;
            }
        }
    }

    public void Destroy()
    {
        Tasking.Tasks.Remove(this);
    }

    public void FinishWithSuccess()
    {
        IsButtonEnable = true;
        TaskState = TaskState.Finished;
        IsDestroyButtonVisible = false;
        ButtonText = MainLang.Remove;
        ProgressIsIndeterminate = false;
        ProgressValue = 100;
        ButtonAction = Destroy;
        foreach (var task in SubTasks) task.FinishWithSuccess();
    }

    public void FinishWithError()
    {
        TaskState = TaskState.Error;
        IsButtonEnable = true;
        IsDestroyButtonVisible = false;
        ButtonText = MainLang.Remove;
        ProgressIsIndeterminate = false;
        ProgressValue = 70;
        ButtonAction = Destroy;
    }

    public void Cancel()
    {
        ProgressValue = 70;
        IsButtonEnable = true;
        IsDestroyButtonVisible = false;
        ButtonText = MainLang.Remove;
        ProgressIsIndeterminate = false;
        ButtonAction = Destroy;
        TaskState = TaskState.Canceled;
        if (SubTasks.Count <= 0) return;
        var t = SubTasks.FirstOrDefault(x => x.TaskState == TaskState.Running);
        t?.Cancel();
    }

    public void CancelWaitFinish()
    {
        IsDestroyButtonVisible = true;
        TaskState = TaskState.Canceling;
        IsCancelRequest = true;
        IsButtonEnable = false;
    }

    public void CancelFinish()
    {
        TaskState = TaskState.Canceled;
        IsButtonEnable = true;
        IsDestroyButtonVisible = false;
        ButtonText = MainLang.Remove;
        ProgressIsIndeterminate = false;
        ProgressValue = 70;
        ButtonAction = Destroy;
        TaskState = TaskState.Canceled;
        if (SubTasks.Count <= 0) return;
        var t = SubTasks.FirstOrDefault(x => x.TaskState == TaskState.Running);
        t?.Cancel();
    }

    public void ButtonActionCommand()
    {
        ButtonAction?.Invoke();
    }

    public void DestroyActionCommand()
    {
        DestroyAction?.Invoke();
    }
}