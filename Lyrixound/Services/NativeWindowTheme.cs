using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Lyrixound.Services;

internal static partial class NativeWindowTheme
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

    public static void ApplyTitleBarTheme(Window window, bool useDarkMode)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var value = useDarkMode ? 1 : 0;
        if (DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref value, sizeof(int)) != 0)
        {
            DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkModeBefore20H1, ref value, sizeof(int));
        }
    }

    public static void BindTitleBarTheme(Window window, ThemeService themeService)
    {
        void Apply() => ApplyTitleBarTheme(window, themeService.IsDarkThemeActive);

        window.SourceInitialized += (_, _) => Apply();

        EventHandler onThemeChanged = (_, _) => Apply();
        themeService.ThemeChanged += onThemeChanged;
        window.Closed += (_, _) => themeService.ThemeChanged -= onThemeChanged;
    }
}
