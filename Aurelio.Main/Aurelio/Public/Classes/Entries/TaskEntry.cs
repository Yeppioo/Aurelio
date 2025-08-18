using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public partial class TaskEntry : ViewModelBase
{
    private readonly Timer _timer;

    public TaskEntry(string name)
    {
        Name = name;
        DestroyAction = Destroy;
        _timer = new Timer(1000);
        _timer.Elapsed += (sender, args) => { Time = Time.Add(TimeSpan.FromSeconds(1)); };
        PropertyChanged += OnPropertyChanged;
    }

    public TaskEntry()
    {
        DestroyAction = Destroy;
        _timer = new Timer(1000);
        _timer.Elapsed += (sender, args) => { Time = Time.Add(TimeSpan.FromSeconds(1)); };
        PropertyChanged += OnPropertyChanged;
    }

     private string _name;
     private TaskState _taskState = TaskState.Waiting;
     private bool _progressIsIndeterminate = true;
     private bool _isCancelRequest;
     private bool _isButtonEnable = true;
     private bool _expanded = true;
     private double _progressValue;
     private Action? _buttonAction;
     private Action? _destroyAction;
     private string _buttonText;
     private string _topRightInfoText;
     private string _bottomLeftInfoText;
     private bool _isDestroyButtonVisible;
     private TimeSpan _time = TimeSpan.Zero;
    public ObservableCollection<TaskEntry> SubTasks { get; set; } = [];
    public ObservableCollection<OperateButtonEntry> OperateButtons { get; set; } = [];

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public TaskState TaskState
    {
        get => _taskState;
        set => SetField(ref _taskState, value);
    }

    public bool ProgressIsIndeterminate
    {
        get => _progressIsIndeterminate;
        set => SetField(ref _progressIsIndeterminate, value);
    }

    public bool IsCancelRequest
    {
        get => _isCancelRequest;
        set => SetField(ref _isCancelRequest, value);
    }

    public bool IsButtonEnable
    {
        get => _isButtonEnable;
        set => SetField(ref _isButtonEnable, value);
    }

    public bool Expanded
    {
        get => _expanded;
        set => SetField(ref _expanded, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetField(ref _progressValue, value);
    }

    public Action? ButtonAction
    {
        get => _buttonAction;
        set => SetField(ref _buttonAction, value);
    }

    public Action? DestroyAction
    {
        get => _destroyAction;
        set => SetField(ref _destroyAction, value);
    }

    public string ButtonText
    {
        get => _buttonText;
        set => SetField(ref _buttonText, value);
    }

    public string TopRightInfoText
    {
        get => _topRightInfoText;
        set => SetField(ref _topRightInfoText, value);
    }

    public string BottomLeftInfoText
    {
        get => _bottomLeftInfoText;
        set => SetField(ref _bottomLeftInfoText, value);
    }

    public bool IsDestroyButtonVisible
    {
        get => _isDestroyButtonVisible;
        set => SetField(ref _isDestroyButtonVisible, value);
    }

    public TimeSpan Time
    {
        get => _time;
        set => SetField(ref _time, value);
    }



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
            BeginTimer();
        else
            StopTimer();
    }

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
            var currentIndex = SubTasks.IndexOf(runningTask);
            var nextTask = SubTasks.Skip(currentIndex + 1).FirstOrDefault(t => t.TaskState == TaskState.Waiting);

            if (nextTask != null)
                // 有下一个任务，设置为运行中
                nextTask.TaskState = TaskState.Running;
        }
        else
        {
            // 没有运行中的任务，将第一个等待中的任务设置为运行中
            var firstWaitingTask = SubTasks.FirstOrDefault(t => t.TaskState == TaskState.Waiting);
            if (firstWaitingTask != null) firstWaitingTask.TaskState = TaskState.Running;
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
        foreach (var task in SubTasks)
            if(task.TaskState == TaskState.Running)
                task.FinishWithError();
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

    public void SetProgressValue(double progress)
    {
        if (progress < 0)
        {
            ProgressIsIndeterminate = true;
            ProgressValue = 0;
            return;
        }

        ProgressIsIndeterminate = false;
        ProgressValue = progress;
    }

    public void SetBottomLeftText(string text)
    {
        BottomLeftInfoText = text;
    }

    public void SetTopRightText(string text)
    {
        TopRightInfoText = text;
    }
}