using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Views.Main;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Ursa.Controls;

namespace Aurelio.Public.Module.Service;

public static class TabDragDropService
{
    private static Window? _sourceWindow;
    private static Point _dragStartPoint;
    private static readonly List<Window> _registeredWindows = new();

    public static bool IsDragging { get; private set; }

    public static TabEntry? DraggedTab { get; private set; }

    public static void RegisterWindow(Window window)
    {
        if (!_registeredWindows.Contains(window))
        {
            _registeredWindows.Add(window);
            window.Closed += (s, e) => _registeredWindows.Remove(window);
        }
    }

    public static void UnregisterWindow(Window window)
    {
        _registeredWindows.Remove(window);
    }

    private static async Task SafeUIOperation(Func<Task> operation)
    {
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
                await operation();
            else
                await Dispatcher.UIThread.InvokeAsync(operation);
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            Debug.WriteLine($"UI Operation Error: {ex.Message}");
        }
    }

    public static void StartDrag(TabEntry tab, Window sourceWindow, Point startPoint)
    {
        DraggedTab = tab;
        _sourceWindow = sourceWindow;
        _dragStartPoint = startPoint;
        IsDragging = true;
    }

    public static void EndDrag()
    {
        DraggedTab = null;
        _sourceWindow = null;
        IsDragging = false;
    }

    public static void HandleDrop(Point dropPoint, Window targetWindow)
    {
        if (DraggedTab == null || _sourceWindow == null) return;

        try
        {
            // Check if dropping on the same window
            if (_sourceWindow == targetWindow)
                HandleReorderInSameWindow(dropPoint, targetWindow);
            else
                HandleTransferBetweenWindows(targetWindow);
        }
        finally
        {
            EndDrag();
        }
    }

    private static void HandleReorderInSameWindow(Point dropPoint, Window window)
    {
        if (DraggedTab == null) return;

        var tabs = GetTabsCollection(window);
        if (tabs == null) return;

        var currentIndex = tabs.IndexOf(DraggedTab);
        if (currentIndex < 0) return;

        var tabsList = GetTabsList(window);
        if (tabsList == null) return;

        var newIndex = GetDropIndex(dropPoint, tabsList, tabs.Count);

        // Ensure valid index and different from current
        if (newIndex >= 0 && newIndex < tabs.Count && newIndex != currentIndex)
        {
            // Store the currently selected tab to preserve selection
            TabEntry? currentlySelectedTab = null;
            if (window is MainWindow mainWindow)
                currentlySelectedTab = mainWindow.ViewModel.SelectedTab;
            else if (window is TabWindow tabWindow) currentlySelectedTab = tabWindow.ViewModel.SelectedTab;

            // Use dispatcher for thread safety
            Dispatcher.UIThread.Post(() =>
            {
                tabs.Move(currentIndex, newIndex);

                // Restore the original selection after reordering
                if (currentlySelectedTab != null)
                {
                    if (window is MainWindow mainWindow)
                        mainWindow.ViewModel.SelectedTab = currentlySelectedTab;
                    else if (window is TabWindow tabWindow) tabWindow.ViewModel.SelectedTab = currentlySelectedTab;
                }
            });
        }
    }

    private static void HandleTransferBetweenWindows(Window targetWindow)
    {
        if (DraggedTab == null || _sourceWindow == null) return;

        // Ensure we're not transferring to the same window
        if (_sourceWindow == targetWindow) return;

        var tabToTransfer = DraggedTab;
        var sourceWindow = _sourceWindow;

        // Allow settings tabs to be transferred to any window
        // Multiple settings tabs are now permitted across different windows

        // Use async operation to avoid layout manager conflicts
        Dispatcher.UIThread.Post(async () =>
        {
            // Remove from source window
            RemoveTabFromWindow(tabToTransfer, sourceWindow);

            // Reduced delay for better responsiveness
            await Task.Delay(25);

            // Refresh content to avoid layout conflicts
            tabToTransfer.RefreshContent();

            // Add to target window
            AddTabToWindow(tabToTransfer, targetWindow);

            // Bring target window to front
            targetWindow.Activate();
            targetWindow.BringIntoView();

            // Close source window if it's a TabWindow with no tabs
            if (sourceWindow is TabWindow sourceTabWindow && !sourceTabWindow.ViewModel.HasTabs)
            {
                await Task.Delay(100);
                sourceTabWindow.Close();
            }
        });
    }

    public static void HandleDetachToNewWindow(Point screenPoint)
    {
        if (DraggedTab == null || _sourceWindow == null) return;

        var tabToDetach = DraggedTab;
        var sourceWindow = _sourceWindow;

        // Allow settings tab to be detached to new window
        // Each window can have its own settings tab

        // Use async operation to avoid layout manager conflicts
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                // Remove from source window
                RemoveTabFromWindow(tabToDetach, sourceWindow);

                // Reduced delay for better responsiveness
                await Task.Delay(25);

                // Refresh content to avoid layout conflicts
                tabToDetach.RefreshContent();

                // Create new TabWindow
                var newWindow = new TabWindow();

                // Position the new window near the drop point, but ensure it's on screen
                var targetX = Math.Max(0, (int)screenPoint.X - 400); // Center the window around the drop point
                var targetY = Math.Max(0, (int)screenPoint.Y - 50);

                // Get screen bounds to ensure window stays on screen
                var screens = newWindow.Screens.All;
                if (screens.Any())
                {
                    var primaryScreen = screens.First();
                    var maxX = primaryScreen.WorkingArea.Width - 800; // Assume minimum window width
                    var maxY = primaryScreen.WorkingArea.Height - 450; // Assume minimum window height

                    targetX = Math.Min(targetX, maxX);
                    targetY = Math.Min(targetY, maxY);
                }

                newWindow.Position = new PixelPoint(targetX, targetY);

                RegisterWindow(newWindow);
                newWindow.Show();

                // Reduced delay for better responsiveness
                await Task.Delay(50);
                newWindow.AddTab(tabToDetach);

                // Close source window if it's a TabWindow with no tabs
                if (sourceWindow is TabWindow sourceTabWindow && !sourceTabWindow.ViewModel.HasTabs)
                {
                    await Task.Delay(100);
                    sourceTabWindow.Close();
                }
            }
            finally
            {
                EndDrag();
            }
        });
    }

    private static Control? GetTabsList(Window window)
    {
        return window.FindControl<Control>("NavMenu");
    }

    private static int GetDropIndex(Point dropPoint, Control tabsList, int tabCount)
    {
        // Find the SelectionList control
        var selectionList = tabsList as SelectionList;
        if (selectionList == null) return -1;

        // Get the items panel (StackPanel)
        var itemsPanel = selectionList.FindDescendantOfType<StackPanel>();
        if (itemsPanel == null) return -1;

        // If dropping before the first item
        if (dropPoint.X < 0) return 0;

        // Find the tab item at the drop position
        for (var i = 0; i < itemsPanel.Children.Count && i < tabCount; i++)
        {
            var child = itemsPanel.Children[i];
            if (child is Control control)
            {
                var bounds = control.Bounds;
                var centerX = bounds.X + bounds.Width / 2;

                // If drop point is before the center of this item, insert here
                if (dropPoint.X < centerX) return i;
            }
        }

        // If we get here, drop at the end
        return Math.Max(0, tabCount - 1);
    }


    private static void RemoveTabFromWindow(TabEntry tab, Window window)
    {
        // Use dispatcher to ensure UI operations happen on the correct thread
        Dispatcher.UIThread.Post(() =>
        {
            if (window is MainWindow mainWindow)
            {
                var wasSelected = mainWindow.ViewModel.SelectedTab == tab;
                mainWindow.ViewModel.Tabs.Remove(tab);

                // If the removed tab was selected, select the last remaining tab
                if (wasSelected) mainWindow.ViewModel.SelectedTab = mainWindow.ViewModel.Tabs.LastOrDefault();
            }
            else if (window is TabWindow tabWindow)
            {
                var wasSelected = tabWindow.ViewModel.SelectedTab == tab;
                tabWindow.ViewModel.RemoveTab(tab);

                // If the removed tab was selected, select the last remaining tab
                if (wasSelected) tabWindow.ViewModel.SelectedTab = tabWindow.ViewModel.Tabs.LastOrDefault();
            }
        });
    }

    private static void AddTabToWindow(TabEntry tab, Window window)
    {
        // Use dispatcher to ensure UI operations happen on the correct thread
        Dispatcher.UIThread.Post(() =>
        {
            if (window is MainWindow mainWindow)
                mainWindow.ViewModel.CreateTab(tab);
            else if (window is TabWindow tabWindow) tabWindow.AddTab(tab);
        });
    }

    private static ObservableCollection<TabEntry>? GetTabsCollection(Window window)
    {
        return window switch
        {
            MainWindow mainWindow => mainWindow.ViewModel.Tabs,
            TabWindow tabWindow => tabWindow.ViewModel.Tabs,
            _ => null
        };
    }

    public static Window? FindWindowAtPoint(Point screenPoint)
    {
        foreach (var window in _registeredWindows.Where(w => w.IsVisible))
        {
            var windowBounds = new Rect(window.Position.ToPoint(1.0), window.Bounds.Size);
            if (windowBounds.Contains(screenPoint)) return window;
        }

        return null;
    }

    public static bool IsPointOutsideAllWindows(Point screenPoint)
    {
        var targetWindow = FindWindowAtPoint(screenPoint);

        // Consider it outside if no window found, or if found window is the source window
        // and the point is outside the tab area
        if (targetWindow == null) return true;

        if (targetWindow == _sourceWindow)
        {
            // Check if point is outside the tab navigation area
            var navRoot = targetWindow.FindControl<Control>("NavRoot");
            if (navRoot != null)
            {
                var localPoint = targetWindow.PointToClient(new PixelPoint((int)screenPoint.X, (int)screenPoint.Y));
                var navBounds = navRoot.Bounds;
                return !navBounds.Contains(localPoint);
            }
        }

        return false;
    }

    // Settings tab enforcement methods removed to allow multiple settings tabs
    // across different windows simultaneously
}