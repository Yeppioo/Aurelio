using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Overlay;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Ursa.Controls;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Aurelio.Views.Main;

public partial class TabWindow : WindowBase
{
    private DateTime _lastShiftPressTime;

    public TabWindow()
    {
#if DEBUG
        InitializeComponent(attachDevTools: false);
#else
        InitializeComponent();
#endif
        Notification = new WindowNotificationManager(GetTopLevel(this));
        Toast = new WindowToastManager(GetTopLevel(this));
        Notification.Position = NotificationPosition.BottomRight;
        Toast.MaxItems = 2;
        DialogHost.HostId = $"DialogHost_{DateTime.Now}";
        DataContext = ViewModel;
        NewTabButton.DataContext = ViewModel;
        BindEvents();
        TabDragDropService.RegisterWindow(this);
    }

    public TabWindowViewModel ViewModel { get; set; } = new();
    public ObservableCollection<TabEntry> Tabs => ViewModel.Tabs;
    public TabEntry? SelectedTab => ViewModel.SelectedTab;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Data.DesktopType == DesktopType.Linux ||
            Data.DesktopType == DesktopType.FreeBSD ||
            (Data.DesktopType == DesktopType.Windows &&
             Environment.OSVersion.Version.Major < 10))
        {
            IsManagedResizerVisible = true;
            SystemDecorations = SystemDecorations.None;
            Root.CornerRadius = new CornerRadius(0);
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaToDecorationsHint = true;
        }
        else if (Data.DesktopType == DesktopType.MacOs)
        {
            SystemDecorations = SystemDecorations.Full;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
            ExtendClientAreaToDecorationsHint = true;
            TitleRoot.Margin = new Thickness(65, 0, 0, 0);
            TitleBar.IsCloseBtnShow = false;
            TitleBar.IsMinBtnShow = false;
            TitleBar.IsMaxBtnShow = false;
            NavRoot.Margin = new Thickness(125, 0, 15, 0);
        }
        else
        {
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaToDecorationsHint = true;
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Transfer remaining tabs back to main window if any exist
        if (ViewModel.HasTabs && App.UiRoot != null)
        {
            var tabsToTransfer = Tabs.ToList();

            // Use dispatcher to ensure proper UI thread handling
            Dispatcher.UIThread.Post(async () =>
            {
                foreach (var tab in tabsToTransfer)
                {
                    ViewModel.RemoveTab(tab);

                    // Small delay and refresh to avoid layout conflicts
                    await Task.Delay(25);
                    tab.RefreshContent();

                    App.UiRoot.ViewModel.CreateTab(tab);
                }
            });
        }

        // Unregister from drag service
        TabDragDropService.UnregisterWindow(this);
    }

    private void BindEvents()
    {
        NewTabButton.Click += NewTabButton_Click;
        Closing += OnClosing;
        ViewModel.TabsEmptied += OnTabsEmptied;
        NavScrollViewer.ScrollChanged += (_, _) => { ViewModel.IsTabMaskVisible = NavScrollViewer.Offset.X > 0; };
        if (Data.DesktopType == DesktopType.MacOs)
        {
            PropertyChanged += (_, e) =>
            {
                var platform = TryGetPlatformHandle();
                if (platform is null) return;
                var nsWindow = platform.Handle;
                if (nsWindow == IntPtr.Zero) return;
                try
                {
                    MacOsWindowHandler.RefreshTitleBarButtonPosition(nsWindow);
                    MacOsWindowHandler.HideZoomButton(nsWindow);

                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            };
        }
        KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.LeftShift or Key.RightShift)) return;
            if ((DateTime.Now - _lastShiftPressTime).TotalMilliseconds < 300)
            {
                var options = new DialogOptions()
                {
                    ShowInTaskBar = false,
                    IsCloseButtonVisible = false,
                    StartupLocation = WindowStartupLocation.Manual,
                    CanDragMove = true,
                    StyleClass = "aggregate-search"
                };
                Dialog.ShowCustom<AggregateSearchDialog, AggregateSearchDialog>(new AggregateSearchDialog(),
                    this.GetVisualRoot() as Window, options: options);
            }

            _lastShiftPressTime = DateTime.Now;
        };
    }

    private void TabItem_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed) return;
        var c = (TabEntry)((Border)sender).Tag;
        c.CloseInWindow(this);
    }

    private void NavScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            scrollViewer.Offset = new Vector(
                scrollViewer.Offset.X + e.Delta.Y * 20, // 调整乘数以控制滚动速度
                scrollViewer.Offset.Y
            );
            e.Handled = true;
        }
    }

    private void NavScrollViewer_PointerEntered(object? sender, PointerEventArgs e)
    {
        NavScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
    }

    private void NavScrollViewer_PointerExited(object? sender, PointerEventArgs e)
    {
        NavScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
    }

    public void CreateTab(TabEntry tab)
    {
        ViewModel.CreateTab(tab);
    }

    public void AddTab(TabEntry tab)
    {
        ViewModel.AddTab(tab);
    }

    public void RemoveTab(TabEntry tab)
    {
        ViewModel.RemoveTab(tab);
    }

    private void OnTabsEmptied()
    {
        // Close the window when no tabs remain
        Close();
    }

    private void NewTabButton_Click(object? sender, RoutedEventArgs e)
    {
        // Check if this window already has a settings tab
        var hasSettingsTab = ViewModel.Tabs.Any(t => t.Tag == "setting");

        if (!hasSettingsTab)
        {
            // Create a new settings tab for this window
            // You may need to adjust this based on how settings tabs are created in your app
            // For now, I'll add a placeholder - you should replace this with your actual settings tab creation logic

            // Example: Create settings tab (replace with your actual implementation)
            // var settingsTab = new TabEntry("设置", "setting", new SettingsTabPage());
            // ViewModel.CreateTab(settingsTab);
        }
    }
}