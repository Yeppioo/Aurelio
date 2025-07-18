using Ursa.Controls;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioWindow 
{
    public WindowNotificationManager Notification { get; set; }
    public WindowToastManager Toast { get; set; }
    public Control RootElement { get; set; }
    public UrsaWindow Window { get; set; }
}