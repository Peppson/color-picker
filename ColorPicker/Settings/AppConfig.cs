using System.Reflection;

namespace ColorPicker.Settings;

public static class AppConfig
{    
    // Overrides
    public static readonly bool? IsEnabledOverride = null;

    // Constants
    public const int InitialZoomLevel = 27;
    public const double MinZoomLevel = 11; // Uneven needed for px centering
    public const double MaxZoomLevel = 91;
    public const bool InitialIsAlwaysOnTop = true;
    public const bool InitialIsMinimized = false;
    public const int MessageDuration = 3000;

    public static readonly string VersionNumber = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3) ??
        throw new InvalidOperationException("Failed to get version number");
}
