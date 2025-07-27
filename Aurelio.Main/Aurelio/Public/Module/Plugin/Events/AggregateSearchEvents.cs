using Aurelio.Public.Classes.Entries;

namespace Aurelio.Public.Module.Plugin.Events;

public class AggregateSearchEvents
{
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
}