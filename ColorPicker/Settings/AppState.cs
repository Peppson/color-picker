using ColorPicker.Components;

namespace ColorPicker.Settings;

public static class Appstate
{   
    // States
    public static bool IsFirstBoot { get; set; } = false;
    public static bool IsAlwaysOnTop { get; set; } = false;
    public static bool IsMinimize { get; set; } = false;
    public static bool GlobalKeybind { get; set; } = false;
    public static double WindowTop { get; set; }
    public static double WindowLeft { get; set; }
    
    // Settings
    public static bool CaptureOnSelf { get; set; } = AppConfig.CaptureColorOnSelf;
    public static bool SetWindowPosOnStartup { get; set; } = AppConfig.SetWindowPosOnStartup;
    public static bool BootWithCaptureEnabled { get; set; } = AppConfig.BootWithCaptureEnabled;





    public static void Load()
    {
        WindowTop = Properties.Settings.Default.WindowTop;
        WindowLeft = Properties.Settings.Default.WindowLeft;
        
        //IsFirstBoot = Properties.Settings.Default.IsFirstBoot;
    }

    public static void Save(double top, double left)
    {
        Properties.Settings.Default.WindowTop = top;
        Properties.Settings.Default.WindowLeft = left;

        //Properties.Settings.Default.IsFirstBoot = IsFirstBoot;
        //Properties.Settings.Default.ColorType = ColorPicker.GetColorType();

        Properties.Settings.Default.Save();
    }

























}
