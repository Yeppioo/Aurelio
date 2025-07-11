using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Enum;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class TaskEntry : ReactiveObject
{
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string? Description { get; set; }
    [Reactive] public StreamGeometry? Icon { get; set; }
    [Reactive] public Control StateIcon { get; set; }

    [Reactive] public TaskState TaskState { get; set; } = TaskState.Waiting;
    [Reactive] public bool ProgressIsIndeterminate { get; set; } = true;
    [Reactive] public double ProgressValue { get; set; }
    public ObservableCollection<TaskEntry> SubTasks { get; set; } = [];

    public TaskEntry()
    {
        StateIcon = GetStateIcon();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TaskState))
            {
                StateIcon = GetStateIcon();
            }
        };
    }

    public Control GetStateIcon()
    {
        return TaskState switch
        {
            TaskState.Running => new ProgressRing() { Width = 16, Height = 20, Margin = new Thickness(-2, 1, 0, -1) },
            TaskState.Waiting => new PathIcon
            {
                Data = Geometry.Parse(
                    "M432 256c0 17.7-14.3 32-32 32L48 288c-17.7 0-32-14.3-32-32s14.3-32 32-32l352 0c17.7 0 32 14.3 32 32z"),
                Width = 14
            },
            TaskState.Canceled => new PathIcon
            {
                Data = Geometry.Parse(
                    "M342.6 150.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0L192 210.7 86.6 105.4c-12.5-12.5-32.8-12.5-45.3 0s-12.5 32.8 0 45.3L146.7 256 41.4 361.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0L192 301.3 297.4 406.6c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L237.3 256 342.6 150.6z"),
                Width = 12, Margin = new Thickness(0, 0, 2, 0)
            },
            TaskState.Canceling => new ProgressRing() { Width = 16, Height = 20, Margin = new Thickness(-2, 1, 0, -1) },
            TaskState.Paused => new PathIcon
            {
                Data = Geometry.Parse(
                    "M432 256c0 17.7-14.3 32-32 32L48 288c-17.7 0-32-14.3-32-32s14.3-32 32-32l352 0c17.7 0 32 14.3 32 32z"),
                Width = 14
            },
            TaskState.Error => new PathIcon
            {
                Data = Geometry.Parse(
                    "M342.6 150.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0L192 210.7 86.6 105.4c-12.5-12.5-32.8-12.5-45.3 0s-12.5 32.8 0 45.3L146.7 256 41.4 361.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0L192 301.3 297.4 406.6c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L237.3 256 342.6 150.6z"),
                Width = 12, Margin = new Thickness(0, 0, 2, 0)
            },
            TaskState.Finished => new PathIcon
            {
                Data = Geometry.Parse(
                    "M438.6 105.4c12.5 12.5 12.5 32.8 0 45.3l-256 256c-12.5 12.5-32.8 12.5-45.3 0l-128-128c-12.5-12.5-12.5-32.8 0-45.3s32.8-12.5 45.3 0L160 338.7 393.4 105.4c12.5-12.5 32.8-12.5 45.3 0z"),
                Width = 14
            },
            _ => new PathIcon { Data = Icon }
        };
    }
}