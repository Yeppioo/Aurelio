using Aurelio.Public.Module.Plugin.Events;
using Aurelio.Public.Module.Service;

namespace Aurelio.Public.Module.App.Services;

public class BindingAppEvents
{
    public static void Main()
    {
        PublicEvents.UpdateAggregateSearchEntries += (_,_) => AggregateSearch.UpdateAggregateSearchEntries();
    }
}