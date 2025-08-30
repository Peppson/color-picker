using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;

namespace ColorPicker.Services;

public static class ScreenCaptureService
{
    private const int SRCCOPY = 0x00CC0020;



    public static BitmapSource GetRegion(int x, int y, int width, int height)
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
                _ = Win32Api.ReleaseDC(IntPtr.Zero, hdcSrc);
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



    /*
    // Simple thread-local buffer to avoid allocations
    [ThreadStatic]
    private static Bitmap? _buffer;

    public static BitmapSource GetRegion(int x, int y, int width, int height)
    {
        // Reuse buffer if same size, otherwise create new
        if (_buffer?.Width != width || _buffer?.Height != height)
        {
            _buffer?.Dispose();
            _buffer = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        using var g = Graphics.FromImage(_buffer);
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

        IntPtr hBitmap = _buffer.GetHbitmap();
        try
        {
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze(); // Freeze for performance
            return source;
        }
        finally
        {
            Win32Api.DeleteObject(hBitmap);
        }
    
    */




}
