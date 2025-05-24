using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Aurelio.Public.Controls;

public partial class TitleBar : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TitleBar, string>(nameof(Title), "");

    public static readonly StyledProperty<bool> IsCloseBtnExitAppProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsCloseBtnExitApp));

    public static readonly StyledProperty<bool> IsCloseBtnShowProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsCloseBtnShow), true);

    public static readonly StyledProperty<bool> IsMaxBtnShowProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsMaxBtnShow), true);
    
    public static readonly StyledProperty<bool> IsMinBtnShowProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsMinBtnShow), true);

    private DateTime? lastClickTime;

    public TitleBar()
    {
        InitializeComponent();
        CloseButton.Click += CloseButton_Click;
        MaximizeButton.Click += MaximizeButton_Click;
        MinimizeButton.Click += MinimizeButton_Click;
        MoveDragArea.PointerPressed += MoveDragArea_PointerPressed;
        Loaded += (_, _) =>
        {
            TitleText.Text = Title;
            CloseButton.IsVisible = IsCloseBtnShow;
            MaximizeButton.IsVisible = IsMaxBtnShow;
            MinimizeButton.IsVisible = IsMinBtnShow;
        };
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsCloseBtnExitApp
    {
        get => GetValue(IsCloseBtnExitAppProperty);
        set => SetValue(IsCloseBtnExitAppProperty, value);
    }

    public bool IsCloseBtnShow
    {
        get => GetValue(IsCloseBtnShowProperty);
        set => SetValue(IsCloseBtnShowProperty, value);
    }

    public bool IsMaxBtnShow
    {
        get => GetValue(IsMaxBtnShowProperty);
        set => SetValue(IsMaxBtnShowProperty, value);
    }
    public bool IsMinBtnShow
    {
        get => GetValue(IsMinBtnShowProperty);
        set => SetValue(IsMinBtnShowProperty, value);
    }

    private void MoveDragArea_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            var control = sender as Grid;
            if (control != null)
            {
                var window = control.GetVisualRoot() as Window;
                window.BeginMoveDrag(e);
            }

            if (IsMaxBtnShow && lastClickTime.HasValue && (DateTime.Now - lastClickTime.Value).TotalMilliseconds < 300)
            {
                lastClickTime = null;
                var window = this.GetVisualRoot() as Window;
                if (window != null)
                    window.WindowState = window.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
            }
            else
            {
                lastClickTime = DateTime.Now;
            }

            e.Handled = true;
        }
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var window = button.GetVisualRoot() as Window;
            if (window != null) window.WindowState = WindowState.Minimized;
        }
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var window = button.GetVisualRoot() as Window;
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized)
                    window.WindowState = WindowState.Normal;
                else
                    window.WindowState = WindowState.Maximized;
            }
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            if (IsCloseBtnExitApp)
            {
                Environment.Exit(0);
            }
            else
            {
                var window = button.GetVisualRoot() as Window;
                if (window != null) window.Hide();
            }
        }
    }

    public void AddButton(Button btn)
    {
        Panel.Children.Add(btn);
        Separator.IsVisible = true;
    }
}