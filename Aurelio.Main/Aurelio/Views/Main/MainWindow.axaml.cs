using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Classes.Interfaces;
using Aurelio.Public.Classes.Setting;
using Aurelio.Public.Module.Service;
using Aurelio.Public.Module.Ui;
using Aurelio.Public.Module.Ui.Helper;
using Aurelio.Public.ViewModels;
using Aurelio.Views.Overlay;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using HotAvalonia;
using Ursa.Controls;
using MoreButtonMenu = Aurelio.Views.Main.Pages.Instance.MoreButtonMenu;
using TaskCenter = Aurelio.Views.Main.Pages.Template.TaskCenter;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Aurelio.Views.Main;

public partial class MainWindow : UrsaWindow, IAurelioWindow
{
    public MainWindow()
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
        Toast.MaxItems = 2;
        DataContext = ViewModel;
        NewTabButton.DataContext = ViewModel;
        Window = this;
        TabDragDropService.RegisterWindow(this);
#if RELEASE
        BindEvents();
        InitTitleBar();
#endif
    }

    public MainViewModel ViewModel { get; set; } = new();
    public ObservableCollection<TabEntry> Tabs => ViewModel.Tabs;
    private DateTime _lastShiftPressTime;
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
            Separator.IsVisible = false;
        }
        else
        {
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaToDecorationsHint = true;
        }

        Setter.SetBackGround(Data.SettingEntry.BackGround, this);
    }

#if DEBUG
    [AvaloniaHotReload]
#endif
    private void InitTitleBar()
    {
        var c = new MoreButtonMenu();
        var menu = (MenuFlyout)c.MainControl!.Flyout;
        MoreButton.Flyout = menu;
        MoreButton.DataContext = new MoreButtonMenuCommands();
        SettingButton.Click += async (_, _) => { await OpenSettingPage(); };
    }

    private async Task OpenSettingPage()
    {
        var (otherWindow, otherSettingsTab) = TabDragDropService.FindSettingsTabInOtherWindows();
        var hasSettingsInOtherWindow = otherWindow != null && otherSettingsTab != null;
        if (hasSettingsInOtherWindow)
        {
            await TabDragDropService.RemoveSettingsTabFromOtherWindowsAsync();
            await Task.Delay(50);
        }

        var existingTab = Tabs.FirstOrDefault(x => x.Tag == "setting");

        if (existingTab == null)
        {
            var newTab = new TabEntry(ViewModel.SettingTabPage)
            {
                Tag = "setting"
            };
            Tabs.Add(newTab);
            ViewModel.SelectedTab = newTab;
        }
        else
        {
            // Use existing tab in main window
            if (SelectedTab == existingTab)
            {
                _ = ViewModel.SettingTabPage.Animate();
                return;
            }

            ViewModel.SelectedTab = existingTab;
        }
    }

#if DEBUG
    [AvaloniaHotReload]
#endif
    private void BindEvents()
    {
        Application.Current.ActualThemeVariantChanged +=
            (_, _) => Setter.SetBackGround(Data.SettingEntry.BackGround, this);
        Closing += OnMainWindowClosing;
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
        Data.SettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(SettingEntry.BackGround) &&
                e.PropertyName != nameof(SettingEntry.BackGroundImgData) &&
                e.PropertyName != nameof(SettingEntry.BackGroundColor)) return;
            Setter.SetBackGround(Data.SettingEntry.BackGround, this);
        };
        NavScrollViewer.ScrollChanged += (_, _) => { ViewModel.IsTabMaskVisible = NavScrollViewer.Offset.X > 0; };
        Loaded += (_, _) =>
        {
            RenderOptions.SetTextRenderingMode(this, TextRenderingMode.SubpixelAntialias); // 字体渲染模式
            RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.MediumQuality); // 图片渲染模式
            RenderOptions.SetEdgeMode(this, EdgeMode.Antialias); // 形状渲染模式
        };
        TitleBarContainer.SizeChanged += (_, _) =>
        {
            NavRoot.Margin = new Thickness((Data.DesktopType == DesktopType.MacOs ? 125 : 80), 0,
                TitleBarContainer.Bounds.Width + (Data.DesktopType == DesktopType.MacOs ? 20 : 85), 0);
        };
        FocusInfoBorder.PointerPressed += async (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                var vis = ((Control)s)!.GetVisualRoot();
                var host = vis is TabWindow w
                    ? w.DialogHost.HostId
                    : "MainWindow";
                _ = OpenTaskDrawer(host!);
                return;
            }

            if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
            {
                if ((s as Control)!.GetVisualRoot() is TabWindow window)
                    window.CreateTab(new TabEntry(new TaskCenter()));
                else
                    App.UiRoot.CreateTab(new TabEntry(new TaskCenter()));
                return;
            }

            if (Tasking.Tasks.Count == 0)
            {
                ViewModel.SettingTabPage.SelectedItem = ViewModel.SettingTabPage.Nav.Items[1] as SelectionListItem;
                ViewModel.SettingTabPage.DefaultNav = 1;
                await OpenSettingPage();
            }
            else
            {
                var vis = ((Control)s)!.GetVisualRoot();
                var host = vis is TabWindow w
                    ? w.DialogHost.HostId
                    : "MainWindow";
                _ = OpenTaskDrawer(host!);
            }
        };
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
        c.Close();
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

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        // If this is the main window closing and there are other TabWindows open,
        // we should handle the scenario appropriately
        TabDragDropService.UnregisterWindow(this);
        Environment.Exit(0);
    }

    public WindowNotificationManager Notification { get; set; }
    public WindowToastManager Toast { get; set; }
    public Control RootElement { get; set; }
    public UrsaWindow Window { get; set; }
}