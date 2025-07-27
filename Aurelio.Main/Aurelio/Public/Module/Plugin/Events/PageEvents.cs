using Ursa.Controls;

namespace Aurelio.Public.Module.Plugin.Events;

public class PageEvents
{
    public static event EventsHandler.NavInitHandler? PageNavInit;

    internal static void OnPageNavInit(SelectionList nav)
    {
        PageNavInit?.Invoke(nav);
    }
}