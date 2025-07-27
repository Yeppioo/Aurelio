using Aurelio.Public.Classes.Entries;
using Avalonia.Input;

namespace Aurelio.Public.Module.Plugin.Events;

public class AppEvents
{
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