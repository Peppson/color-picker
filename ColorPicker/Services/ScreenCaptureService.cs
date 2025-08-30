using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;

namespace ColorPicker.Services;

public static class ScreenCaptureService
{
    private const int SRCCOPY = 0x00CC0020;

    public static BitmapSource CaptureRegion(int x, int y, int width, int height)
    {
        using var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            IntPtr hdcDest = g.GetHdc();
            IntPtr hdcSrc = Win32Api.GetDC(IntPtr.Zero);

            try
            {
                int srcX = x - (width / 2);
                int srcY = y - (height / 2);

                Win32Api.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, srcX, srcY, SRCCOPY);
            }
            finally
            {
                Win32Api.ReleaseDC(IntPtr.Zero, hdcSrc);
                g.ReleaseHdc(hdcDest);
            }
        }

        IntPtr hBitmap = bmp.GetHbitmap();
        try
        {
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            Win32Api.DeleteObject(hBitmap);
        }
    }

    // Future methods:
    // public static BitmapSource CaptureFullScreen()
    // public static BitmapSource CaptureWindow(IntPtr hWnd)
}


