using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ColorPicker.Components;

public partial class ColorPicker : UserControl
{
    // Import Windows API
    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    [DllImport("user32.dll")]
    static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }






    private DispatcherTimer _timer;
    private SolidColorBrush _brush = new SolidColorBrush();
    private Color _lastColor;



    public ColorPicker()
    {
        InitializeComponent();
        //SetWindowSizeAndPosition();
        ColorPreview.Fill = _brush;

        // Setup a timer to update ~240 Hz (4 ms)
        /* _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(4.16);
        _timer.Tick += UpdateColor;
        _timer.Start(); */
    }
    
    private void UpdateColor(object? sender, EventArgs e)
    {
        if (!GetCursorPos(out POINT p))
            return;

        IntPtr hdc = GetDC(IntPtr.Zero);
        uint colorRef = GetPixel(hdc, p.X, p.Y);
        ReleaseDC(IntPtr.Zero, hdc);

        // Extract RGB
        byte r = (byte)(colorRef & 0x000000FF);
        byte g = (byte)((colorRef & 0x0000FF00) >> 8);
        byte b = (byte)((colorRef & 0x00FF0000) >> 16);

        var newColor = Color.FromRgb(r, g, b);

        // Only update if color changed
        if (newColor == _lastColor) return;
        _lastColor = newColor;

        // Update brush + text
        _brush.Color = newColor;
        ColorText.Text = $"R: {r}, G: {g}, B: {b}";
    }
}
