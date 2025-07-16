using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.Value;
using Avalonia.Media;
using Avalonia.Threading;
using Irihi.Avalonia.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Const;

public class Tasking : ReactiveObject , IDialogContext
{
    private static Tasking? _instance;

    public Tasking()
    {
        Data.SettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Data.SettingEntry.UsingMinecraftAccount))
                UpdateDisplay();
        };
        Tasks.CollectionChanged += (_, _) => TasksChanged();
        UpdateDisplay();
    }

    [Reactive] public string FocusInfoText { get; set; }
    [Reactive] public SolidColorBrush FocusInfoColor { get; set; }

    public static Tasking Instance
    {
        get { return _instance ??= new Tasking(); }
    }

    public static ObservableCollection<TaskEntry> Tasks { get; } = [];

    public static TaskEntry CreateTask(string name)
    {
        var task = new TaskEntry
        {
            Name = name,
            TaskState = TaskState.Waiting
        };
        Tasks.Add(task);
        return task;
    }

    public void UpdateDisplay()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Tasks.Count == 0)
            {
                FocusInfoText = Data.SettingEntry.UsingMinecraftAccount.Name;
                FocusInfoColor = Data.SettingEntry.UsingMinecraftAccount.AccountType switch
                {
                    Setting.AccountType.Microsoft => SolidColorBrush.Parse("#00FF40"),
                    Setting.AccountType.Offline => SolidColorBrush.Parse("#FFA500"),
                    Setting.AccountType.ThirdParty => SolidColorBrush.Parse("#35FFF6"),
                    _ => SolidColorBrush.Parse("#00FF40")
                };
            }
            else if (Tasks.Count == 1)
            {
                FocusInfoText = Tasks[0].Name;
                FocusInfoColor = new SolidColorBrush(Converter.TaskStateToColor(Tasks[0].TaskState));
            }
            else
            {
                FocusInfoText = MainLang.TaskingTip.Replace("{num}", Tasks.Count.ToString());
                FocusInfoColor = new SolidColorBrush(Converter.TaskStateToColor(Tasks.Last().TaskState));
            }
        });
    }

    private void TasksChanged()
    {
        UpdateDisplay();
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}