using Aurelio.Public.Classes.Entries;
using Avalonia.Input;

namespace Aurelio.Public.Module.Plugin.Events;

public class EventsHandler
{
    public delegate void ExecuteAggregateSearchHandler(object? sender, AggregateSearchEntry entry);
    public delegate void AppDragDropHandler(object? sender, DragEventArgs e);
}