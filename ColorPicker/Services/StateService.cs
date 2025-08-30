using System.Windows;
using ColorPicker.Models;
using ColorPicker.Settings;

namespace ColorPicker.Services;

public static class State
{
    // States
    public static bool IsFirstBoot { get; set; }
    public static bool IsEnabled { get; set; }
    public static bool GlobalKeybind { get; set; } // todo save
    public static bool SetWindowPosOnStartup { get; set; }
    public static double WindowTop { get; set; }
    public static double WindowLeft { get; set; }
    public static bool CaptureOnSelf { get; set; }
    public static ColorTypes CurrentColorType { get; set; }
    public static bool IsAlwaysOnTop { get; set; } = AppConfig.InitialIsAlwaysOnTop;
    public static bool IsMinimized { get; set; } = AppConfig.InitialIsMinimized;

    // Refs
    public static Window MainWindow { get; private set; } = null!;
    public static System.Drawing.Rectangle MainWindowPos { get; private set; }
    
    private static bool _isResetting = false;

    // Methods
    public static void Init(Window window)
    {
        MainWindow = window;
        LoadFromMemory();

        #if !RELEASE
            StartupLogDebug();
        #endif
    }

    public static void LoadFromMemory()
    { 
        IsFirstBoot = Properties.Settings.Default.IsFirstBoot;
        //GlobalKeybind = Properties.Settings.Default.GlobalKeybind;
        WindowTop = Properties.Settings.Default.WindowTop;
        WindowLeft = Properties.Settings.Default.WindowLeft;
        IsEnabled = Properties.Settings.Default.BootWithCaptureEnabled;

        CurrentColorType = ColorService.StringToColorType(Properties.Settings.Default.ColorType);
        CaptureOnSelf = Properties.Settings.Default.CaptureColorOnSelf;
        SetWindowPosOnStartup = Properties.Settings.Default.SetWindowPosOnStartup;
    }

    public static void Save()
    {   
        if (_isResetting) return; // Don't save if reset


        Properties.Settings.Default.IsFirstBoot = false; // todo

        //Properties.Settings.Default.GlobalKeybind = GlobalKeybind;

        Properties.Settings.Default.WindowTop = MainWindow.Top;
        Properties.Settings.Default.WindowLeft = MainWindow.Left;

        Properties.Settings.Default.BootWithCaptureEnabled = false; // todo

        //Properties.Settings.Default.ColorType = MainWindow.ColorPicker.GetColorType();

        Properties.Settings.Default.Save();
    }

    public static void Reset()
    {   
        Console.WriteLine("Resetting settings...");
        _isResetting = true;

        Properties.Settings.Default.Reset();
        Properties.Settings.Default.Save();

        // todo
        var currentExe = Environment.ProcessPath ?? throw new InvalidOperationException("Could not get process path");
        System.Diagnostics.Process.Start(currentExe);
    
        Application.Current.Shutdown();
    }

    public static void UpdateMainWindowPos()
    {
        MainWindowPos = new System.Drawing.Rectangle(
            (int)MainWindow.Left,
            (int)MainWindow.Top,
            (int)MainWindow.Width,
            (int)MainWindow.Height
        );
    }

    private static void StartupLogDebug()
    {
        Console.WriteLine($"\n--- DEBUG v{AppConfig.VersionNumber} ---");
        Console.WriteLine($"- IsFirstBoot: {IsFirstBoot}");
        Console.WriteLine($"- WindowTop: {WindowTop}");
        Console.WriteLine($"- WindowLeft: {WindowLeft}");
        Console.WriteLine($"- SetWindowPosOnStartup: {SetWindowPosOnStartup}");
        Console.WriteLine($"- IsEnabled: {IsEnabled}");
        Console.WriteLine($"- CurrentColorType: {CurrentColorType}");
        Console.WriteLine($"- CaptureOnSelf: {CaptureOnSelf}");
        Console.WriteLine("");
    }
}
