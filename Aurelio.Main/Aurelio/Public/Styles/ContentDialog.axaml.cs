using Avalonia.Input;
using Avalonia.VisualTree;

namespace Aurelio.Public.Styles;

public partial class ContentDialog : ResourceDictionary
{
    private void Container_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Mouse) return;
        if (sender is Control control)
        {
            if (e.Source == sender)
            {
                var window = control.GetVisualRoot() as Window;
                window?.BeginMoveDrag(e);
                e.Handled = true;
            }
        }
    }
}