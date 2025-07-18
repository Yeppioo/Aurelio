using Aurelio.Public.Classes.Entries;
using Aurelio.Views.Main;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using TaskCenter = Aurelio.Views.Main.Pages.TaskCenter;

namespace Aurelio.Public.Styles.Override.Template;

public class Drawer : ResourceDictionary
{
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        ClearDrawer();
        if ((sender as Control)!.GetVisualRoot() is TabWindow window)
            window.CreateTab(new TabEntry(new TaskCenter()));
        else
            App.UiRoot.CreateTab(new TabEntry(new TaskCenter()));
    }
}