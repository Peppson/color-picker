using System.Reflection;

namespace ColorPicker.Settings;

public static class Config
{
    // Overrides
    public static readonly bool? IsEnabledOverride = null;
    public const bool ShowDebugbutton = false;
    public const bool BootWithSettings = false;
    public const bool BootWithWelcomeWindow = true;

    // Constants
    public const int MaxSamplesPerSecond = 120;
    public const int InitialZoomLevel = 27;
    public const double MinZoomLevel = 11; // Uneven needed for px centering
    public const double MaxZoomLevel = 91;
    public const int MessageDuration = 3000;
    public const bool InitialIsMinimized = false;

    public static readonly string VersionNumber = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3) ??
        throw new InvalidOperationException("Failed to get version number");
        
    public const string GithubLink = "https://github.com/Peppson/color-pick";
}
