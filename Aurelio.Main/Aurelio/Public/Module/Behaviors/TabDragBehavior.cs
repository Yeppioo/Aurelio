using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Module.Services;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Aurelio.Public.Module.Behaviors;

public class TabDragBehavior
{
    private static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TabDragBehavior, Control, bool>("IsEnabled");

    private static readonly AttachedProperty<TabDragHandler?> HandlerProperty =
        AvaloniaProperty.RegisterAttached<TabDragBehavior, Control, TabDragHandler?>("Handler");

    public static bool GetIsEnabled(Control control) => control.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Control control, bool value) => control.SetValue(IsEnabledProperty, value);

    static TabDragBehavior()
    {
        IsEnabledProperty.Changed.Subscribe(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.Sender is Control control)
        {
            var oldHandler = control.GetValue(HandlerProperty);
            oldHandler?.Detach();

            if (args.NewValue.Value)
            {
                var newHandler = new TabDragHandler(control);
                control.SetValue(HandlerProperty, newHandler);
                newHandler.Attach();
            }
            else
            {
                control.SetValue(HandlerProperty, null);
            }
        }
    }

    private class TabDragHandler
    {
        private readonly Control _control;
        private bool _isDragging;
        private Point _startPoint;
        private TabEntry? _tabEntry;

        public TabDragHandler(Control control)
        {
            _control = control;
        }

        public void Attach()
        {
            _control.PointerPressed += OnPointerPressed;
            _control.PointerMoved += OnPointerMoved;
            _control.PointerReleased += OnPointerReleased;
        }

        public void Detach()
        {
            _control.PointerPressed -= OnPointerPressed;
            _control.PointerMoved -= OnPointerMoved;
            _control.PointerReleased -= OnPointerReleased;
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(_control).Properties.IsLeftButtonPressed)
            {
                _startPoint = e.GetPosition(_control);
                _tabEntry = _control.Tag as TabEntry;

                if (_tabEntry != null)
                {
                    // Don't capture pointer immediately, let normal click handling work first
                    // We'll capture it only when drag starts
                }
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_tabEntry != null)
            {
                var currentPoint = e.GetPosition(_control);
                var distance = Point.Distance(_startPoint, currentPoint);

                if (!_isDragging && distance > 5) // Reduced drag threshold for better sensitivity
                {
                    _isDragging = true;
                    var window = _control.FindAncestorOfType<Window>();
                    if (window != null && _tabEntry != null)
                    {
                        // Now capture the pointer for dragging
                        e.Pointer.Capture(_control);

                        TabDragDropService.StartDrag(_tabEntry, window, _startPoint);

                        // Add minimal visual feedback - only opacity change
                        _control.Opacity = 0.6;
                    }
                }

                if (_isDragging && e.Pointer.Captured == _control)
                {
                    // Handle drag feedback and detection of drop zones
                    HandleDragMove(e);
                }
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDragging)
            {
                HandleDrop(e);
                _isDragging = false;
                _control.Opacity = 1.0;
                e.Pointer.Capture(null);
            }
            else if (_tabEntry != null)
            {
                // If we didn't drag, this was just a click - let it through for tab selection
                // Don't set e.Handled = true here to allow normal click processing
            }

            _tabEntry = null;
        }

        private void HandleDragMove(PointerEventArgs e)
        {
            var window = _control.FindAncestorOfType<Window>();
            if (window == null) return;

            var screenPoint = window.PointToScreen(e.GetPosition(window)).ToPoint(1.0);
            var targetWindow = TabDragDropService.FindWindowAtPoint(screenPoint);

            // Update drag preview position
            TabDragDropService.UpdateDragPreview(screenPoint);

            // Minimal visual feedback - only opacity changes
            if (TabDragDropService.IsPointOutsideAllWindows(screenPoint))
            {
                // Visual feedback for detachment (creating new window)
                _control.Opacity = 0.4;
            }
            else if (targetWindow != null && targetWindow != window)
            {
                // Visual feedback for transfer to another window
                _control.Opacity = 0.5;
            }
            else
            {
                // Visual feedback for reordering within same window
                _control.Opacity = 0.6;
            }
        }

        private void HandleDrop(PointerReleasedEventArgs e)
        {
            var window = _control.FindAncestorOfType<Window>();
            if (window == null) return;

            var screenPoint = window.PointToScreen(e.GetPosition(window)).ToPoint(1.0);
            var targetWindow = TabDragDropService.FindWindowAtPoint(screenPoint);

            if (targetWindow != null)
            {
                // Drop on existing window
                var localPoint = targetWindow.PointToClient(new PixelPoint((int)screenPoint.X, (int)screenPoint.Y));

                // Convert to the tab area coordinates
                var navMenu = targetWindow.FindControl<Control>("NavMenu");
                if (navMenu != null)
                {
                    var screenPointPixel = targetWindow.PointToScreen(localPoint);
                    var navMenuPoint = navMenu.PointToClient(screenPointPixel);
                    TabDragDropService.HandleDrop(navMenuPoint, targetWindow);
                }
                else
                {
                    TabDragDropService.HandleDrop(localPoint, targetWindow);
                }
            }
            else
            {
                // Drop outside all windows - create new window
                TabDragDropService.HandleDetachToNewWindow(screenPoint);
            }
        }
    }
}
