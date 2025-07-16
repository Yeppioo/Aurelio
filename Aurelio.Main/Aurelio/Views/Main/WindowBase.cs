using Ursa.Controls;

namespace Aurelio.Views.Main;

public class WindowBase : UrsaWindow
{
    public WindowNotificationManager Notification { get; set; }
    public WindowToastManager Toast { get; set; }
}