using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Aurelio.Public.Classes.Entries;

public class OperateButtonEntry(string content, Action<object> action) : ReactiveObject
{
    [Reactive] public object? Content { get; set; } = content;
    [Reactive] public Action<object>? Action { get; set; } = action;

    public void Command(object sender)
    {
        Action?.Invoke(sender);
    }
}