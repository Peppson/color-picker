using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ColorPicker.Models;
using FontAwesome.WPF;

namespace ColorPicker.Services;

public static class ColorService
{   

    /* private static string CurrentTextContent { get; set; } = "#FFFFFF";
    public async Task CopyColorToClipboard()
    {
        if (string.IsNullOrEmpty(CurrentTextContent)) return;

        string success = "Copied";
        try
        {
            Clipboard.SetText(CurrentTextContent);
        }
        catch
        {
            success = "Copy failed!";
        }

        await MessageService.ShowAsync(this, success, AppConfig.MessageDuration);
    } */

    public static ColorTypes StringToColorType(string colorType)
    {
        return colorType switch
        {
            "RGB" => ColorTypes.RGB,
            "HEX" => ColorTypes.HEX,
            "HSL" => ColorTypes.HSL,
            "HSV" => ColorTypes.HSV,
            "CMYK" => ColorTypes.CMYK,
            _ => ColorTypes.HEX,
        };
    }
    
    public static System.Windows.Media.Brush GetIconColor(bool isActive)
    {
        return isActive
            ? (System.Windows.Media.Brush)Application.Current.Resources["PrimaryText"]
            : (System.Windows.Media.Brush)Application.Current.Resources["OnTopDisabled"];
    }




























}
