/*

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
} */

//



using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace ColorPicker.Services;

public static class ScreenCaptureService
{
    private const int SRCCOPY = 0x00CC0020;

    private static WriteableBitmap? _reusableBitmap;

    public static BitmapSource GetRegion(int x, int y, int width, int height)
    {
        // Reuse WriteableBitmap to avoid repeated allocations
        if (_reusableBitmap == null || _reusableBitmap.PixelWidth != width || _reusableBitmap.PixelHeight != height)
        {
            _reusableBitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
        }

        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
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

        // Copy pixels into WriteableBitmap
        var rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
        var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

        try
        {
            _reusableBitmap.Lock();
            _reusableBitmap.WritePixels(new Int32Rect(0, 0, width, height), bmpData.Scan0, bmpData.Stride * height, bmpData.Stride);
            _reusableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            _reusableBitmap.Unlock();
        }
        finally
        {
            bmp.UnlockBits(bmpData);
        }

        return _reusableBitmap;
    }
}








