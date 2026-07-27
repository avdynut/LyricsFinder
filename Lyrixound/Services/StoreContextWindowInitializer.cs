using System;

namespace Lyrixound.Services;

/// <summary>
/// Desktop Bridge / WPF apps must bind WinRT UI dialogs (Store purchase) to an HWND.
/// </summary>
internal static class StoreContextWindowInitializer
{
    public static void Initialize(object winrtObject, IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("A valid owner window handle is required for Store purchase UI.");
        }

        // CsWinRT helper — required for projected WinRT objects (direct cast fails).
        WinRT.Interop.InitializeWithWindow.Initialize(winrtObject, hwnd);
    }
}
