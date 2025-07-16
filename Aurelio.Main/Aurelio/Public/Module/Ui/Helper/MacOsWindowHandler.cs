using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Aurelio.Public.Module.Ui.Helper;

[SupportedOSPlatform("MacOS")]
public static class MacOsWindowHandler {
    public static void HideZoomButton(IntPtr nsWindow) {
        var selStandardWindowButton = sel_registerName("standardWindowButton:");
        var selSetHidden = sel_registerName("setHidden:");

        var zoomButton = objc_msgSend_IntPtr_IntPtr(nsWindow, selStandardWindowButton, (IntPtr)2);
        objc_msgSend_Bool(zoomButton, selSetHidden, true);
    }

    public static void RefreshTitleBarButtonPosition(IntPtr nsWindow) {
        var selStandardWindowButton = sel_registerName("standardWindowButton:");
        var selSetFrameOrigin = sel_registerName("setFrameOrigin:");

        if (selStandardWindowButton == IntPtr.Zero || selSetFrameOrigin == IntPtr.Zero) {
            throw new NullReferenceException();
        }

        var closeButton = objc_msgSend_IntPtr_IntPtr(nsWindow, selStandardWindowButton, (IntPtr)0);
        var minimizeButton = objc_msgSend_IntPtr_IntPtr(nsWindow, selStandardWindowButton, (IntPtr)1);
        var zoomButton = objc_msgSend_IntPtr_IntPtr(nsWindow, selStandardWindowButton, (IntPtr)2);

        // 调整按钮位置，使其位于标题栏的左上角，这是macOS的标准位置
        var macButtonX = 20;
        var macButtonY = -1;
        var buttonSpacing = 20;
        
        var newCloseButtonPosition = new CGPoint(macButtonX, macButtonY);
        var newMinimizeButtonPosition = new CGPoint(macButtonX + buttonSpacing, macButtonY);
        var newZoomButtonPosition = new CGPoint(macButtonX + buttonSpacing * 2, macButtonY);

        objc_msgSend_CGPoint(closeButton, selSetFrameOrigin, newCloseButtonPosition);
        objc_msgSend_CGPoint(minimizeButton, selSetFrameOrigin, newMinimizeButtonPosition);
        objc_msgSend_CGPoint(zoomButton, selSetFrameOrigin, newZoomButtonPosition);
    }

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_CGPoint(IntPtr receiver, IntPtr selector, CGPoint arg1);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr objc_msgSend_Bool(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport("libobjc.dylib", EntryPoint = "sel_registerName")]
    public static extern IntPtr sel_registerName(string name);
}

[StructLayout(LayoutKind.Sequential)]
public struct CGPoint {
    public double X;
    public double Y;

    public CGPoint(double x, double y) {
        X = x;
        Y = y;
    }
}