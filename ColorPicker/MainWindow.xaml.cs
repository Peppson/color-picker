using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ColorPicker;

public partial class MainWindow : Window
{
    private const int HOTKEY_ID = 9000;    

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public MainWindow()
    {
        InitializeComponent();
        StateChanged += OnWindowStateChanged;
        SizeChanged += OnWindowSizeOrLocationChanged;
        LocationChanged += OnWindowSizeOrLocationChanged;
        SourceInitialized += OnSourceInitialized;
        Closing += OnWindowClose;

        SetWindowPosition();
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        bool isMinimized = WindowState == WindowState.Minimized;
        ColorPicker.SetIsMinimized(isMinimized);
    }

    private void OnWindowSizeOrLocationChanged(object? sender, EventArgs e)
    {
        ColorPicker.UpdateAppWindowPos(this);
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // Prevent maximize from doubleclick on titlebar
        var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        hwndSource.AddHook(PreventMaximize);

        // Setup Ctrl+Spacebar as global hotkey for pause/resume
        SetupGlobalHotkey();
    }

    private void OnWindowClose(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        Properties.Settings.Default.WindowTop = this.Top;
        Properties.Settings.Default.WindowLeft = this.Left;
        Properties.Settings.Default.ColorType = ColorPicker.GetColorType();
        Properties.Settings.Default.Save();

        // Unregister Ctrl+Spacebar hotkey
        var helper = new WindowInteropHelper(this);
        UnregisterHotKey(helper.Handle, HOTKEY_ID);
    }

    private void SetWindowPosition()
    {
        if (Properties.Settings.Default.WindowTop == 0 || Properties.Settings.Default.WindowLeft == 0)
            return;

        this.Top = Properties.Settings.Default.WindowTop;
        this.Left = Properties.Settings.Default.WindowLeft;
    }

    private void SetupGlobalHotkey()
    {
        const uint CtrlKey  = 0x0002;
        const uint spacebarKey = 0x20;

        var helper = new WindowInteropHelper(this);
        RegisterHotKey(helper.Handle, HOTKEY_ID, CtrlKey , spacebarKey);
        HwndSource source = HwndSource.FromHwnd(helper.Handle);
        source.AddHook(HandleGlobalHotkey);
    }

    private IntPtr PreventMaximize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 0x00A3)
        {
            handled = true;
            return IntPtr.Zero;
        }

        return IntPtr.Zero;
    }

    private IntPtr HandleGlobalHotkey(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;

        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            ColorPicker.ToggleSampling();
            handled = true;
        }

        return IntPtr.Zero;
    }
}
