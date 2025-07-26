namespace Aurelio.Public.Module.Plugin.Events;

public class PublicEvents
{
    public static event EventHandler? UpdateAggregateSearchEntries;

    public static void OnUpdateAggregateSearchEntries()
    {
        UpdateAggregateSearchEntries.Invoke(null, EventArgs.Empty);
    }
}