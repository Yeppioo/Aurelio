using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Module.App.Services;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Main.Pages.Instance;
using Aurelio.Views.Overlay;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Ursa.Controls;
using Notification = System.Reactive.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Aurelio.Views.Main;

public partial class TabWindow : UrsaWindow, IAurelioWindow
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
        RootElement = Root;
        Window = this;
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
        BindKeys.Main(this);

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

        Setter.SetBackGround(Data.SettingEntry.BackGround, this);
    }

    public void TogglePage(string tag, IAurelioTabPage page)
    {
        var existingTab = Tabs.FirstOrDefault(x => x.Tag == tag);

        if (existingTab == null)
        {
            var newTab = new TabEntry(page)
            {
                Tag = tag
            };
            Tabs.Add(newTab);
            ViewModel.SelectedTab = newTab;
        }
        else
        {
            if (SelectedTab == existingTab)
            {
                existingTab.Content.InAnimator.Animate();
                return;
            }

            ViewModel.SelectedTab = existingTab;
        }
    }

    public void OpenSettingPage()
    {
        var existingTab = Tabs.FirstOrDefault(x => x.Tag == "setting");

        if (existingTab == null)
        {
            var newTab = new TabEntry(new SettingTabPage())
            {
                Tag = "setting"
            };
            Tabs.Add(newTab);
            ViewModel.SelectedTab = newTab;
        }
        else
        {
            // Use existing tab in this window
            if (SelectedTab == existingTab)
            {
                existingTab.Content.InAnimator.Animate();
                return;
            }

            ViewModel.SelectedTab = existingTab;
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
        Application.Current.ActualThemeVariantChanged -= CurrentOnActualThemeVariantChanged;
        Data.SettingEntry.PropertyChanged -= SettingEntryOnPropertyChanged;
        TabDragDropService.UnregisterWindow(this);
    }

    private void BindEvents()
    {
        Application.Current.ActualThemeVariantChanged += CurrentOnActualThemeVariantChanged;
        Data.SettingEntry.PropertyChanged += SettingEntryOnPropertyChanged;
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

    private void SettingEntryOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SettingEntry.BackGround) &&
            e.PropertyName != nameof(SettingEntry.BackGroundImgData) &&
            e.PropertyName != nameof(SettingEntry.BackGroundColor)) return;
        Setter.SetBackGround(Data.SettingEntry.BackGround, this);
    }

    private void CurrentOnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        Setter.SetBackGround(Data.SettingEntry.BackGround, this);
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
        // Create a new settings tab for this window
        // Each TabWindow can now have its own settings tab
        var newSettingsTab = new TabEntry(new SettingTabPage())
        {
            Tag = "setting"
        };
        ViewModel.CreateTab(newSettingsTab);
    }

    public WindowNotificationManager Notification { get; set; }
    public WindowToastManager Toast { get; set; }
    public Control RootElement { get; set; }
    public UrsaWindow Window { get; set; }
}