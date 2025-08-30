using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ColorPicker.Services;

public static partial class GlobalHotkeyManager
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HOTKEY_ID = 9000;
    private static HwndSource? _source;
    
    public static void Register(Window window, uint modifiers = 0x0002, uint key = 0x20) // todo get dynamic
    {
        var helper = new WindowInteropHelper(window);

        // Try to set hotkey // todo unregister before new
        if (!RegisterHotKey(helper.Handle, HOTKEY_ID, modifiers, key))
        {
            // todo
            MessageBox.Show("Failed to register global hotkey. It might be already in use by another application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(HandleHotkey);
    }

    public static void UnRegister(Window window)
    {
        if (_source == null) return;

        var helper = new WindowInteropHelper(window);
        UnregisterHotKey(helper.Handle, HOTKEY_ID);
        _source.RemoveHook(HandleHotkey);
        _source = null;
    }

    private static IntPtr HandleHotkey(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;

        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            DB.Print("Global hotkey pressed");
            //ColorPicker.ToggleSampling(); todo
            handled = true;
        }

        return IntPtr.Zero;
    }
}
