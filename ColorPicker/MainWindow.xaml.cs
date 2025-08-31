using System.Windows;
using System.Windows.Interop;
using ColorPicker.Services;

namespace ColorPicker;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Topmost = true;

        SourceInitialized += OnSourceInitialized;
        StateChanged += OnWindowStateChanged;
        SizeChanged += OnWindowSizeOrLocationChanged;
        LocationChanged += OnWindowSizeOrLocationChanged;
        Closing += OnWindowClose;

        State.Init(this);
        SetWindowPosition();
        IsFirstBootWindow();
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
        State.IsMinimized = (WindowState == WindowState.Minimized);
    }

    private void OnWindowSizeOrLocationChanged(object? sender, EventArgs e)
    {
        State.UpdateMainWindowPos();
    }

    private void OnWindowClose(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        State.Save();
        GlobalHotkeyManager.UnRegister(this);
    }

    private void SetWindowPosition()
    {
        if (!State.SetWindowPosOnStartup || State.IsFirstBoot)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        this.Top = State.WindowTop;
        this.Left = State.WindowLeft;
    }

    private void IsFirstBootWindow() // todo
    {
        if (!State.IsFirstBoot) return;

        MessageBox.Show( 
            "Welcome to ColorPicker!\n\n" +
            "To get started, hover over any area of your screen to pick a color.\n\n" +
            "You can change settings by clicking the gear icon in the top-right corner.\n\n" +
            "Enjoy!",
            "Welcome to ColorPicker",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );

        State.IsFirstBoot = false;
    }

    private IntPtr PreventMaximize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = (msg == 0x00A3);
        return IntPtr.Zero;
    }
}
