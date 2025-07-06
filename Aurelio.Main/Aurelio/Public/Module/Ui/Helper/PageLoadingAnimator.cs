using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using SukiUI;
using SukiUI.Helpers;

namespace Aurelio.Public.Module.Ui.Helper;

public class PageLoadingAnimator(Control root, Thickness margin, (double o1, double o2) opacity)
{
    private CancellationTokenSource _opacity;
    private CancellationTokenSource _margin;

    public void Animate()
    {
        if (root.IsAnimating(Visual.OpacityProperty))  _opacity.Cancel();
        if (root.IsAnimating(Layoutable.MarginProperty))  _margin.Cancel();
        _opacity = root.Animate<double>(Visual.OpacityProperty, opacity.o1, opacity.o2);
        _margin = root.Animate(Layoutable.MarginProperty,
            margin, new Thickness(0));
        
        // root.Transitions = [];
        // root.Margin = margin;
        // root.Opacity = opacity.o1;
        // root.Transitions.Add(new DoubleTransition()
        // {
        //     Duration = TimeSpan.FromSeconds(0.25),
        //     Property = Visual.OpacityProperty,
        //     Easing = new QuarticEaseOut()
        // });
        // root.Transitions.Add(new ThicknessTransition()
        // {
        //     Duration = TimeSpan.FromSeconds(0.25),
        //     Property = Layoutable.MarginProperty,
        //     Easing = new QuarticEaseOut()
        // });
        // root.Margin = new Thickness(0);
        // root.Opacity = opacity.o2;
        
        root.IsVisible = true;
    }
}