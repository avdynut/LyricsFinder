using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Lyrixator
{
    public class HotKey : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int _hotKeyMessage = 0x0312;
        private static int _lastId = 12429;

        private readonly int _id;
        private readonly IntPtr _windowHandle;

        public event EventHandler OnHotKeyPressed;

        public HotKey(Window window, KeyModifiers keyModifiers, Key key)
        {
            _id = _lastId++;
            _windowHandle = new WindowInteropHelper(window).Handle;

            var virtualKey = KeyInterop.VirtualKeyFromKey(key);

            if (RegisterHotKey(_windowHandle, _id, (uint)keyModifiers, (uint)virtualKey))
            {
                ComponentDispatcher.ThreadFilterMessage += OnComponentDispatcherThreadFilterMessage;
            }
            else
            {
                Debug.WriteLine("Cannot register hotkey");
            }
        }

        public void Dispose()
        {
            ComponentDispatcher.ThreadFilterMessage -= OnComponentDispatcherThreadFilterMessage;
            UnregisterHotKey(_windowHandle, _id);
        }

        private void OnComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled && msg.message == _hotKeyMessage)
            {
                OnHotKeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}
