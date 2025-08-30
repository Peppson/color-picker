using System.Globalization;
using System.Windows.Data;
using ColorPicker.Models;

namespace ColorPicker.Services;

public static class ColorService
{
    public static  ColorTypes ConvertStringToColorType(string colorType)
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



    



























}
