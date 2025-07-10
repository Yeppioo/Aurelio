using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Langs;
using Aurelio.Public.Module.IO.Local;
using Aurelio.Public.Module.Services;
using Aurelio.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using Material.Icons;
using Ursa.Controls;

namespace Aurelio.Views.Main;

public partial class TabWindow : UrsaWindow
{
    public TabWindowViewModel ViewModel { get; set; } = new();
    public ObservableCollection<TabEntry> Tabs => ViewModel.Tabs;
    public TabEntry? SelectedTab => ViewModel.SelectedTab;

    public TabWindow()
    {
#if DEBUG
        InitializeComponent(attachDevTools: false);
#else
        InitializeComponent();
#endif
        DataContext = ViewModel;
        NewTabButton.DataContext = ViewModel;
        NewTabButton.Click += NewTabButton_Click;
        BindEvents();

        // Register with drag service
        TabDragDropService.RegisterWindow(this);

        // Handle window closing
        Closing += OnClosing;

        // Handle automatic closing when no tabs remain
        ViewModel.TabsEmptied += OnTabsEmptied;
    }

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
        }

        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaToDecorationsHint = true;
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
        NavScrollViewer.ScrollChanged += (_, _) => { ViewModel.IsTabMaskVisible = NavScrollViewer.Offset.X > 0; };
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

    public void CreateTab(TabEntry tab) => ViewModel.CreateTab(tab);

    public void AddTab(TabEntry tab) => ViewModel.AddTab(tab);

    public void RemoveTab(TabEntry tab) => ViewModel.RemoveTab(tab);

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
