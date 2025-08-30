using System.Windows;
using System.Windows.Interop;

namespace ColorPicker.Services;

public static partial class GlobalKeybindManager
{
    private const int HOTKEY_ID = 9000;
    private static HwndSource? _source;
    
    public static void Register(Window window, uint modifiers = 0x0002, uint key = 0x20) // todo get dynamic
    {
        var helper = new WindowInteropHelper(window);

        // Try to set hotkey // todo unregister before new
        if (!Win32Api.RegisterHotKey(helper.Handle, HOTKEY_ID, modifiers, key))
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
        Win32Api.UnregisterHotKey(helper.Handle, HOTKEY_ID);
        _source.RemoveHook(HandleHotkey);
        _source = null;
    }

    private static IntPtr HandleHotkey(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;

        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            Console.WriteLine("Global hotkey pressed");
            //ColorPicker.ToggleSampling(); todo
            handled = true;
        }

        return IntPtr.Zero;
    }
}
