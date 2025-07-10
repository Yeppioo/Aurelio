using Aurelio.ViewModels;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Aurelio.Public.Controls;

public partial class TitleBar : PageMixModelBase
{
    private bool _isCloseBtnExitApp;
    private bool _isCloseBtnHideWindow;
    private bool _isCloseBtnShow = true;
    private bool _isMaxBtnShow = true;
    private bool _isMinBtnShow = true;

    private object _leftContent;
    // public static readonly StyledProperty<string> DataSourceProperty =
    //     AvaloniaProperty.Register<TitleBar, string>(nameof(DataSource));

    // public string DataSource
    // {
    //     get => GetValue(DataSourceProperty);
    //     set => SetValue(DataSourceProperty, value);
    // }

    private string _title;

    public TitleBar()
    {
        InitializeComponent();
        DataContext = this;
        CloseButton.Click += CloseButton_Click;
        MaximizeButton.Click += MaximizeButton_Click;
        MinimizeButton.Click += MinimizeButton_Click;
        MoveDragArea.PointerPressed += MoveDragArea_PointerPressed;
    }

    public Data Data => Data.Instance;

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public object LeftContent
    {
        get => _leftContent;
        set => SetField(ref _leftContent, value);
    }

    public bool IsCloseBtnExitApp
    {
        get => _isCloseBtnExitApp;
        set => SetField(ref _isCloseBtnExitApp, value);
    }

    public bool IsCloseBtnHideWindow
    {
        get => _isCloseBtnHideWindow;
        set => SetField(ref _isCloseBtnHideWindow, value);
    }

    public bool IsCloseBtnShow
    {
        get => _isCloseBtnShow;
        set => SetField(ref _isCloseBtnShow, value);
    }

    public bool IsMaxBtnShow
    {
        get => _isMaxBtnShow;
        set => SetField(ref _isMaxBtnShow, value);
    }

    public bool IsMinBtnShow
    {
        get => _isMinBtnShow;
        set => SetField(ref _isMinBtnShow, value);
    }

    public DateTime? lastClickTime { get; set; }


    private void MoveDragArea_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            if (sender is Grid control)
            {
                var window = control.GetVisualRoot() as Window;
                window.BeginMoveDrag(e);
            }

            if (IsMaxBtnShow && lastClickTime.HasValue && (DateTime.Now - lastClickTime.Value).TotalMilliseconds < 300)
            {
                lastClickTime = null;
                if (this.GetVisualRoot() is Window window)
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
        if (sender is not Button button) return;
        if (button.GetVisualRoot() is Window window) window.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.GetVisualRoot() is not Window window) return;
        window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (IsCloseBtnExitApp)
        {
            Environment.Exit(0);
        }
        else
        {
            if (button.GetVisualRoot() is not Window window) return;
            if (IsCloseBtnHideWindow)
            {
                window.Hide();
            }
            else
            {
                CloseButton.Click -= CloseButton_Click;
                MaximizeButton.Click -= MaximizeButton_Click;
                MinimizeButton.Click -= MinimizeButton_Click;
                MoveDragArea.PointerPressed -= MoveDragArea_PointerPressed;
                window.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
    }
}