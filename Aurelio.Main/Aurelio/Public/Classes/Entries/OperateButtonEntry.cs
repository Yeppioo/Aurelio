using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class OperateButtonEntry(string content, Action action) : ReactiveObject
{
    [Reactive] public object? Content { get; set; } = content;
    [Reactive] public Action? Action { get; set; } = action;

    public void Command()
    {
        Action?.Invoke();
    }
}