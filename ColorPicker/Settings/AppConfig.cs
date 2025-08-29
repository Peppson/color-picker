using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace ColorPicker.Settings;

public static class AppConfig
{
    // Defaults
    public const bool SetWindowPosOnStartup = true;
    public const bool CaptureColorOnSelf = false;
    public const bool BootWithCaptureEnabled = false;
    
    



    public static readonly string VersionNumber = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3) ??
        throw new InvalidOperationException("Failed to get version number");


    

    // keyboard modifiers
    /* const uint MOD_ALT     = 0x0001;
    const uint MOD_CONTROL = 0x0002;
    const uint MOD_SHIFT   = 0x0004;
    const uint MOD_WIN     = 0x0008; */
































}
