using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using SukiUI;
using SukiUI.Helpers;

namespace Aurelio.Public.Module.Ui.Helper;

public class PageLoadingAnimator(Control root, Thickness margin, (double o1, double o2) opacity)
{
    public readonly Control Root = root;
    private CancellationTokenSource _opacity;
    private CancellationTokenSource _margin;

    public async Task Animate()
    {
        await Task.Delay(10);
        if (Root.IsAnimating(Visual.OpacityProperty)) await _opacity.CancelAsync();
        if (Root.IsAnimating(Layoutable.MarginProperty)) await _margin.CancelAsync();
        _opacity = Root.Animate<double>(Visual.OpacityProperty, opacity.o1, opacity.o2);
        _margin = Root.Animate(Layoutable.MarginProperty,
            margin, new Thickness(0));
        Root.IsVisible = true;
    }
}