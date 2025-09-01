using System.Windows;
using System.Windows.Interop;
using ColorPicker.Services;
using ColorPicker.Settings;

namespace ColorPicker;

public partial class MainWindow : Window
{   
    internal Components.Settings? _settingsWindow;

    public Components.Settings SettingsWindow
    {
        get
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new Components.Settings();
                Settings.Content = _settingsWindow;
            }
            return _settingsWindow;
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        this.Topmost = true;

        SourceInitialized += OnSourceInitialized;
        StateChanged += OnWindowStateChanged;
        SizeChanged += OnWindowSizeOrLocationChanged;
        LocationChanged += OnWindowSizeOrLocationChanged;
        Loaded += OnLoaded;
        Closing += OnWindowClose;

        State.Init(this);
        SetWindowPosition();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // Prevent maximize from doubleclick on titlebar
        var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        hwndSource.AddHook(PreventMaximize);

        if (!GlobalHotkeyManager.Register(this, State.GlobalHotkey!)) State.GlobalHotkey = "";
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        IsFirstBootWindow();
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

    private void IsFirstBootWindow() 
    {
        if (!Config.BootWithWelcomeWindow && !State.IsFirstBoot)
            return;

        State.IsEnabled = false;
        var welcomeWindow = new Windows.WelcomeWindow
        {
            Owner = this
        };

        welcomeWindow.ShowDialog();
        State.IsEnabled = true;
    }

    private IntPtr PreventMaximize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = (msg == 0x00A3);
        return IntPtr.Zero;
    }
}
