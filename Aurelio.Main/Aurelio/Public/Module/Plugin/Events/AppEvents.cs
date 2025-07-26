using Aurelio.Public.Classes.Entries;
using Avalonia.Input;

namespace Aurelio.Public.Module.Plugin.Events;

public class AppEvents
{
    public static event EventHandler? BeforeReadSettings;

    internal static void OnBeforeReadSettings()
    {
        BeforeReadSettings?.Invoke(null, EventArgs.Empty);
    }

    public static event EventHandler? BeforeUiLoaded;

    internal static void OnBeforeUiLoaded()
    {
        BeforeUiLoaded?.Invoke(null, EventArgs.Empty);
    }

    public static event EventHandler? AfterUiLoaded;

    internal static void OnAfterUiLoaded()
    {
        AfterUiLoaded?.Invoke(null, EventArgs.Empty);
    }

    public static event EventsHandler.ExecuteAggregateSearchHandler? ExecuteAggregateSearch;

    internal static void OnExecuteAggregateSearch(AggregateSearchEntry entry, Control sender)
    {
        ExecuteAggregateSearch?.Invoke(sender, entry);
    }

    public static event EventHandler? UpdateAggregateSearchEntries;

    internal static void OnUpdateAggregateSearchEntries()
    {
        UpdateAggregateSearchEntries?.Invoke(null, EventArgs.Empty);
    }
    
    public static event EventHandler? SaveSettings;

    internal static void OnSaveSettings()
    {
        SaveSettings?.Invoke(null, EventArgs.Empty);
    }
    
    public static event EventsHandler.AppDragDropHandler? AppDragDrop;

    internal static void OnAppDragDrop(object? sender, DragEventArgs e)
    {
        AppDragDrop?.Invoke(sender, e);
    }
}