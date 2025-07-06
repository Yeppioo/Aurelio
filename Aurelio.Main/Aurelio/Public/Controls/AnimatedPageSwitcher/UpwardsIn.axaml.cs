using Avalonia;
using Avalonia.Controls;
using System.Threading.Tasks;
using System.Threading;
using SukiUI;

namespace Aurelio.Public.Controls.AnimatedPageSwitcher;

public partial class UpwardsInAnimatedPageSwitcher : UserControl
{
    private Control? _oldContentControl;
    private Control? _newContentControl;

    private bool _isAnimating => GetAnimating();

    private bool GetAnimating()
    {
        var res = false;

        if (_newHost != null && !res)
        {
            res = _newHost.IsAnimating(OpacityProperty) ||
                  _newHost.IsAnimating(MarginProperty);
        }
        else if (_oldHost != null && !res)
        {
            res = _oldHost.IsAnimating(OpacityProperty) ||
                  _oldHost.IsAnimating(MarginProperty);
        }
        return res;
    }

    private CancellationTokenSource? _oldOpacity;
    private CancellationTokenSource? _oldMargin;
    private CancellationTokenSource? _newOpacity;
    private CancellationTokenSource? _newMargin;
    
    public UpwardsInAnimatedPageSwitcher()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PageProperty)
        {
            _ = SwitchContent(e.OldValue, e.NewValue);
        }
    }

    public static readonly StyledProperty<object> PageProperty =
        AvaloniaProperty.Register<UpwardsInAnimatedPageSwitcher, object>(nameof(Page));

    public object Page
    {
        get => GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }

    private ContentControl? _oldHost;
    private ContentControl? _newHost;

    private async Task SwitchContent(object? eOldValue, object? eNewValue)
    {
        if (_isAnimating && _oldHost != null)
        {
            Container.Children.Remove(_oldHost);
        }
        _newContentControl = eNewValue as Control;
        _oldContentControl = eOldValue as Control;
        
        await _newMargin?.CancelAsync();
        await _newOpacity?.CancelAsync();
        await _oldMargin?.CancelAsync();
        await _oldOpacity?.CancelAsync();

        if (_newContentControl != null && _oldContentControl != null)
        {
            _oldHost = Container.Children[0] as ContentControl;
            var c = new ContentControl()
            {
                Content = _newContentControl,
                IsVisible = false,
            };
            Container.Children.Add(c);
            _newHost = c;
            await Task.Delay(10);
            _oldOpacity = _oldHost?.Animate<double>(OpacityProperty, 1, 0);
            _oldMargin = _oldHost?.Animate(MarginProperty, 
                new Thickness(0), new Thickness(0,40,0,0));
            _newOpacity = _newHost?.Animate<double>(OpacityProperty, 0, 1);
            _newMargin = _oldHost?.Animate(MarginProperty, 
                new Thickness(0,40,0,0), new Thickness(0));
        }
    }
}