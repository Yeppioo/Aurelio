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
    [Reactive] public string Name { get; set; }
    [Reactive] public TaskState TaskState { get; set; } = TaskState.Waiting;
    [Reactive] public bool ProgressIsIndeterminate { get; set; } = true;
    [Reactive] public bool Expanded { get; set; } = true;
    [Reactive] public double ProgressValue { get; set; }
    public ObservableCollection<TaskEntry> SubTasks { get; set; } = [];
}