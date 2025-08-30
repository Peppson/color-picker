using System.Windows;
using ColorPicker.Components;
using ColorPicker.Models;
using ColorPicker.Services;

namespace ColorPicker.Settings;

public static class Appstate
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

    // Methods
    public static void Init(Window window)
    {
        MainWindow = window;

        IsFirstBoot = Properties.Settings.Default.IsFirstBoot;
        //GlobalKeybind = Properties.Settings.Default.GlobalKeybind;
        WindowTop = Properties.Settings.Default.WindowTop;
        WindowLeft = Properties.Settings.Default.WindowLeft;
        IsEnabled = Properties.Settings.Default.BootWithCaptureEnabled;

        CurrentColorType = ColorService.ConvertStringToColorType(Properties.Settings.Default.ColorType);
        CaptureOnSelf = Properties.Settings.Default.CaptureColorOnSelf;
        SetWindowPosOnStartup = Properties.Settings.Default.SetWindowPosOnStartup;

        #if !RELEASE
            DebugStartupLog();
        #endif
    }

    public static void Save(double top, double left)
    {   
        
        Properties.Settings.Default.IsFirstBoot = false; // todo

        //Properties.Settings.Default.GlobalKeybind = GlobalKeybind;
        Properties.Settings.Default.WindowTop = top;
        Properties.Settings.Default.WindowLeft = left;
        Properties.Settings.Default.BootWithCaptureEnabled = false; // todo

        //Properties.Settings.Default.ColorType = MainWindow.ColorPicker.GetColorType();

        Properties.Settings.Default.Save();
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

    private static void DebugStartupLog()
    {
        DB.Print($"\n--- Initialized v{AppConfig.VersionNumber} ---");
        DB.Print($"- IsFirstBoot: {IsFirstBoot}");
        DB.Print($"- WindowTop: {WindowTop}");
        DB.Print($"- WindowLeft: {WindowLeft}");
        DB.Print($"- SetWindowPosOnStartup: {SetWindowPosOnStartup}");
        DB.Print($"- IsEnabled: {IsEnabled}");
        DB.Print($"- CurrentColorType: {CurrentColorType}");
        DB.Print($"- CaptureOnSelf: {CaptureOnSelf}");
        DB.Print("");
    }
}
