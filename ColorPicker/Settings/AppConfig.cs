using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using ColorPicker.Models;

namespace ColorPicker.Settings;

public static class AppConfig
{
    public static readonly string VersionNumber = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3) ??
        throw new InvalidOperationException("Failed to get version number");
    public const int InitialZoomLevel = 27;
    public const double MinZoomLevel = 11; // Uneven needed for px centering
    public const double MaxZoomLevel = 91;
    public const bool InitialIsAlwaysOnTop = true;
    public const bool InitialIsMinimized = false;
    

    // Overrides
    public static readonly bool? IsEnabledOverride = false; // todo
    
    
    



    


    

    // keyboard modifiers
    /* const uint MOD_ALT     = 0x0001;
    const uint MOD_CONTROL = 0x0002;
    const uint MOD_SHIFT   = 0x0004;
    const uint MOD_WIN     = 0x0008; */
































}
