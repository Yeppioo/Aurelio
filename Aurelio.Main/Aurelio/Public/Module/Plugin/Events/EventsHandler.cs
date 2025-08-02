using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Avalonia.Input;
using Ursa.Controls;

namespace Aurelio.Public.Module.Plugin.Events;

public class EventsHandler
{
    public delegate void ExecuteAggregateSearchHandler(object? sender, AggregateSearchEntry entry);
    public delegate void AppDragDropHandler(object? sender, DragEventArgs e);
    public delegate void OnlySenderHandler(object? sender);
    public delegate Task<bool> AppExitingHandler();
}

public class AppExitingEventArgs : EventArgs
{ 
    public bool Cancel { get; set; }
}