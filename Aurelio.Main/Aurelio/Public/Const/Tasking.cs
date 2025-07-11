using System.Collections.ObjectModel;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Minecraft;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Const;

public class Tasking : ReactiveObject
{
    private static Tasking? _instance;
    [Reactive] public string TaskDisplayText { get; set; }
    [Reactive] public SolidColorBrush TaskDisplayColor { get; set; }
    public static Tasking Instance
    {
        get { return _instance ??= new Tasking(); }
    }
    
    public ObservableCollection<TaskEntry> Tasks { get; } = [];

    public Tasking()
    {
        Data.SettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Data.SettingEntry.UsingMinecraftAccount))
                UpdateDisplay();
        };
        Tasks.CollectionChanged += (_, _) => TasksChanged();
    }
    
    private void UpdateDisplay()
    {
        if (Tasks.Count == 0)
        {
            TaskDisplayText = Data.SettingEntry.UsingMinecraftAccount.Name;
            TaskDisplayColor = Data.SettingEntry.UsingMinecraftAccount.AccountType switch
            {
                Setting.AccountType.Microsoft => SolidColorBrush.Parse("#00FF40"),
                Setting.AccountType.Offline => SolidColorBrush.Parse("#FFA500"),
                Setting.AccountType.ThirdParty => SolidColorBrush.Parse("#35FFF6"),
                _ => SolidColorBrush.Parse("#00FF40")
            };
        }
    }

    private void TasksChanged()
    {
    }
}