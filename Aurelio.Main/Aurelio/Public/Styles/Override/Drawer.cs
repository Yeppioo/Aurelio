using Aurelio.Public.Classes.Entries;
using Aurelio.Views.Main.Template;
using Avalonia.Interactivity;

namespace Aurelio.Public.Styles.Override;

public class Drawer : ResourceDictionary
{
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        ClearDrawer();
        App.UiRoot.CreateTab(new TabEntry(new TaskCenter()));
    }
}