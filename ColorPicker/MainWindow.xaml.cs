using System.Windows;
using System.Windows.Interop;
using ColorPicker.Services;
using ColorPicker.Settings;

namespace ColorPicker;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;
        StateChanged += OnWindowStateChanged;
        SizeChanged += OnWindowSizeOrLocationChanged;
        LocationChanged += OnWindowSizeOrLocationChanged;
        Closing += OnWindowClose;

        Appstate.Init(this);
        SetWindowPosition();
        //IsFirstBootWindow(); 
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // Prevent maximize from doubleclick on titlebar
        var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        hwndSource.AddHook(PreventMaximize);

        GlobalHotkeyManager.Register(this);
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        Appstate.IsMinimized = (WindowState == WindowState.Minimized);
    }

    private void OnWindowSizeOrLocationChanged(object? sender, EventArgs e)
    {
        Appstate.UpdateMainWindowPos();
    }

    private void OnWindowClose(object? sender, System.ComponentModel.CancelEventArgs e)
    {   
        Appstate.Save(this.Top, this.Left);
        GlobalHotkeyManager.UnRegister(this);
    }

    private void SetWindowPosition()
    {   
        if (!Appstate.SetWindowPosOnStartup || Appstate.IsFirstBoot)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        this.Top = Appstate.WindowTop;
        this.Left = Appstate.WindowLeft;
    }

    private IntPtr PreventMaximize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // todo spams
        handled = (msg == 0x00A3);
        return IntPtr.Zero;
    }
}
