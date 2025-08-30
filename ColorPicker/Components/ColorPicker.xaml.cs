using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ColorPicker.Models;
using ColorPicker.Services;
using ColorPicker.Settings;
using FontAwesome.WPF;

namespace ColorPicker.Components;

public partial class ColorPicker : UserControl, INotifyPropertyChanged
{
    [LibraryImport("user32.dll")]
    private static partial IntPtr GetDC(IntPtr hWnd);

    [LibraryImport("gdi32.dll")]
    private static partial uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    [LibraryImport("user32.dll")]
    private static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }





    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    private bool _isMessageOpen = false;
    private DateTime _lastUpdate = DateTime.UtcNow;
    private POINT _lastMousePos;
    private SolidColorBrush _currentBrush = new(Colors.White);
    private SolidColorBrush _invertedBrush = new(Colors.Black);
    

    private static string CurrentTextContent { get; set; } = "#FFFFFF";

    
    private ColorTypes _colorType;
    public ColorTypes CurrentColorType // HEX, RGB...
    {
        get => _colorType;
        set
        {
            if (_colorType != value)
            {
                _colorType = value;
                OnPropertyChanged(nameof(CurrentColorType));
            }
        }
    }

    
    private int _zoomLevel = AppConfig.InitialZoomLevel;
    public int ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (_zoomLevel != value)
            {
                _zoomLevel = value;
                OnPropertyChanged(nameof(ZoomLevel));
                OnPropertyChanged(nameof(ZoomPercent));
                UpdateZoomView(_lastMousePos, _zoomLevel);
            }
        }
    }

    
    public int ZoomPercent => 
        (_zoomLevel - (int)AppConfig.MinZoomLevel) * 100
        / ((int)AppConfig.MaxZoomLevel - (int)AppConfig.MinZoomLevel); // Ugly af


    private CancellationTokenSource? _messageCts;







    private Border? _slider1;
    private RepeatButton? _slider2;
    private RepeatButton? _slider3;
    private void RegisterSliderParts()
    {
        ZoomSlider.ApplyTemplate();

        if (ZoomSlider.Template.FindName("PART_Track", ZoomSlider) is Track track)
        {
            track.DecreaseRepeatButton.ApplyTemplate();
            track.IncreaseRepeatButton.ApplyTemplate();
            track.Thumb.ApplyTemplate();

            _slider2 = track.DecreaseRepeatButton as RepeatButton;
            _slider3 = track.IncreaseRepeatButton as RepeatButton;

            if (track.Thumb.Template.FindName("PART_ThumbBorder", track.Thumb) is Border thumbBorder)
            {
                _slider1 = thumbBorder;
            }
        }
    }






    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetCursorPos(int X, int Y);
    private bool _isDragging = false;
    private POINT _dragStartMouse;
    private POINT _dragStartPos;

    private void ZoomImage_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            Mouse.OverrideCursor = Cursors.None;

            if (GetCursorPos(out _dragStartMouse))
                _dragStartPos = _lastMousePos;

            ZoomImage.CaptureMouse();
        }
    }

    private void ZoomImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && GetCursorPos(out POINT currentMouse))
        {
            int dx = currentMouse.X - _dragStartMouse.X;
            int dy = currentMouse.Y - _dragStartMouse.Y;
            _lastMousePos.X = _dragStartPos.X + dx;
            _lastMousePos.Y = _dragStartPos.Y + dy;

            UpdateZoomView(_lastMousePos, ZoomLevel);
            UpdateColors(_lastMousePos);
        }
    }

    private void ZoomImage_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ZoomImage.ReleaseMouseCapture();

            // Set mouse pos back where we started
            SetCursorPos(_dragStartMouse.X, _dragStartMouse.Y);
            Mouse.OverrideCursor = null;
        }
    }









    public ColorPicker()
    {
        InitializeComponent();
        DataContext = this;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        CompositionTarget.Rendering += OnNewFrame!;

        ZoomImage.MouseWheel += ZoomImage_MouseWheel;
        ZoomImage.MouseDown += ZoomImage_MouseDown;
        ZoomImage.MouseMove += ZoomImage_MouseMove;
        ZoomImage.MouseUp += ZoomImage_MouseUp;
    }

    private void OnNewFrame(object sender, EventArgs e)
    {
        // Clamp to max 120fps color updates
        const int minInterval = 1000 / 120;

        if (!Appstate.IsEnabled || Appstate.IsMinimized)
            return;

        if (DateTime.UtcNow < _lastUpdate.AddMilliseconds(minInterval))
            return;
        _lastUpdate = DateTime.UtcNow;

        if (!GetCursorPos(out POINT p))
            return;

        if (Appstate.MainWindowPos.Contains(p.X, p.Y))
            return;

        if (_lastMousePos.X == p.X && _lastMousePos.Y == p.Y)
            return;
        _lastMousePos = p;

        UpdateColors(p);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Window window = Window.GetWindow(this)!;
        window.PreviewKeyDown += Keyboard_Click;

        CurrentColorType = ColorService.ConvertStringToColorType(Properties.Settings.Default.ColorType); // todo

        if (AppConfig.IsEnabledOverride != null)
            Appstate.IsEnabled = AppConfig.IsEnabledOverride.Value;

        SetIsEnabledIcon(Appstate.IsEnabled);    
        RegisterSliderParts();
        UpdateColorsStatic(); 
        DB.Print($"Loaded color type: {CurrentColorType}");
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Window window = Window.GetWindow(this)!;
        window.PreviewKeyDown -= Keyboard_Click;
    }

    private void DropdownButton_Click(object sender, MouseButtonEventArgs e)
    {
        DropdownButton.ContextMenu.PlacementTarget = DropdownButton;
        DropdownButton.ContextMenu.Placement = PlacementMode.Bottom;
        DropdownButton.ContextMenu.HorizontalOffset = 0;
        DropdownButton.ContextMenu.VerticalOffset = 0;
        DropdownButton.ContextMenu.IsOpen = true;
    }

    private void DropdownMouse_Click(object sender, MouseButtonEventArgs e)
    {
        var mousePos = e.GetPosition((IInputElement)sender);

        DropdownButton.ContextMenu.PlacementTarget = (UIElement)sender;
        DropdownButton.ContextMenu.Placement = PlacementMode.Relative;
        DropdownButton.ContextMenu.HorizontalOffset = mousePos.X;
        DropdownButton.ContextMenu.VerticalOffset = mousePos.Y;
        DropdownButton.ContextMenu.IsOpen = true;
    }

    private void DropdownMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is ColorTypes colorType)
        {
            CurrentColorType = colorType;
            DB.Print($"Color type set: {CurrentColorType}");
            UpdateColorsStatic();
        }
        e.Handled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        _ = CopyColorToClipboard();
        UpdateMessageColor(_invertedBrush);
        e.Handled = true;
    }

    private void ToggleEnabled_Click(object sender, MouseButtonEventArgs e)
    {
        ToggleSampling();
        e.Handled = true;
    }
    
    private void Keyboard_Click(object sender, KeyEventArgs e)
    {
        // CTRL + C
        if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            _ = CopyColorToClipboard();
            UpdateMessageColor(_invertedBrush);
            e.Handled = true;
            return;
        }
        // Spacebar 
        else if (e.Key == Key.Space)
        {
            ToggleSampling();
            e.Handled = true;
            return;
        }
        // Arrow keys
        else if (e.Key == Key.Left)
            _lastMousePos.X--;
        else if (e.Key == Key.Right)
            _lastMousePos.X++;
        else if (e.Key == Key.Up)
            _lastMousePos.Y--;
        else if (e.Key == Key.Down)
            _lastMousePos.Y++;
           
        UpdateColors(_lastMousePos);
        e.Handled = true;
    }

    private void ZoomImage_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        const int step = 2;

        if (e.Delta > 0)
            ZoomLevel = Math.Min(ZoomLevel + step, (int)AppConfig.MaxZoomLevel);
        else
            ZoomLevel = Math.Max(ZoomLevel - step, (int)AppConfig.MinZoomLevel);

        e.Handled = true;
    }

    public void ToggleSampling()
    {
        Appstate.IsEnabled = !Appstate.IsEnabled;
        SetIsEnabledIcon(Appstate.IsEnabled);
        DB.Print($"Color sampling: {Appstate.IsEnabled}");
    }

    public void SetSampling(bool enabled)
    {
        Appstate.IsEnabled = enabled;
        SetIsEnabledIcon(Appstate.IsEnabled);
        DB.Print($"Color sampling: {Appstate.IsEnabled}");
    }

    private void SetIsEnabledIcon(bool running)
    {
        IsEnabledIcon.Icon = running ? FontAwesomeIcon.Pause : FontAwesomeIcon.Play;
    }

    





















    public async Task ShowMessageAsync(string message, int durationMs)
    {
        _isMessageOpen = true;

        // Cancel any previous messages
        _messageCts?.Cancel();
        _messageCts = new CancellationTokenSource();
        var token = _messageCts.Token;

        Message.Text = message;

        // Fade in
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1));
        Message.BeginAnimation(OpacityProperty, fadeIn);

        // Fade out
        try
        {
            await Task.Delay(durationMs, token);
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            Message.BeginAnimation(OpacityProperty, fadeOut);
        }
        catch (TaskCanceledException) { }

        _isMessageOpen = false;
    }


   












    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(
        IntPtr hdcDest,
        int nXDest,
        int nYDest,
        int nWidth,
        int nHeight,
        IntPtr hdcSrc,
        int nXSrc,
        int nYSrc,
        int dwRop);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(IntPtr hObject);

    private const int SRCCOPY = 0x00CC0020;


    private static BitmapSource CaptureRegion(int x, int y, int width, int height)
    {
        using var bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            IntPtr hdcDest = g.GetHdc();
            IntPtr hdcSrc = GetDC(IntPtr.Zero);

            try
            {
                int srcX = x - (width / 2);
                int srcY = y - (height / 2);

                BitBlt(hdcDest, 0, 0, width, height, hdcSrc, srcX, srcY, SRCCOPY);
            }
            finally
            {
                _ = ReleaseDC(IntPtr.Zero, hdcSrc);
                g.ReleaseHdc(hdcDest);
            }
        }

        IntPtr hBitmap = bmp.GetHbitmap();
        try
        {
            var source = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }



















    private void UpdateColors(POINT p)
    {
        uint colorRef = GetColorAtPos(p);

        byte r = (byte)(colorRef & 0x000000FF);
        byte g = (byte)((colorRef & 0x0000FF00) >> 8);
        byte b = (byte)((colorRef & 0x00FF0000) >> 16);

        // Zoom
        UpdateZoomView(p, _zoomLevel);

        if (IsSameColor(_currentBrush, r, g, b)) return;

        _currentBrush.Color = Color.FromRgb(r, g, b);
        _invertedBrush.Color = GetInvertedColor(r, g, b);

        // UI
        UpdatePreviewView(_currentBrush);
        UpdateTextContent(r, g, b);
        UpdateThemeColors(_invertedBrush);
    }

    private void UpdateColorsStatic()
    {
        byte r = _currentBrush.Color.R;
        byte g = _currentBrush.Color.G;
        byte b = _currentBrush.Color.B;

        UpdatePreviewView(_currentBrush);
        UpdateTextContent(r, g, b);
        UpdateThemeColors(_invertedBrush);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetColorAtPos(POINT p)
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        uint colorRef = GetPixel(hdc, p.X, p.Y);
        _ = ReleaseDC(IntPtr.Zero, hdc);

        return colorRef;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSameColor(SolidColorBrush brush, byte r, byte g, byte b)
    {
        return brush.Color.R == r &&
                brush.Color.G == g &&
                brush.Color.B == b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color GetInvertedColor(byte r, byte g, byte b)
    {
        byte invR = (byte)(255 - r);
        byte invG = (byte)(255 - g);
        byte invB = (byte)(255 - b);

        return Color.FromRgb(invR, invG, invB);
    }

    private void UpdateTextContent(byte r, byte g, byte b)
    {
        string content, type;
        switch (CurrentColorType)
        {
            case ColorTypes.RGB:
                content = RGB(r, g, b); type = "RGB"; break;
            case ColorTypes.HEX:
                content = HEX(r, g, b); type = "HEX"; break;
            case ColorTypes.HSL:
                content = HSL(r, g, b); type = "HSL"; break;
            case ColorTypes.HSV:
                content = HSV(r, g, b); type = "HSV"; break;
            case ColorTypes.CMYK:
                content = CMYK(r, g, b); type = "CMYK"; break;
            default:
                DB.Print("Unknown color type");
                content = "";
                type = "";
                break;
        }

        ColorTextType.Text = type;
        ColorText.Text = content;
        CurrentTextContent = content;
    }

    private void UpdatePreviewView(SolidColorBrush brush)
    {
        ColorPreview.Fill = brush;
    }

    private void UpdateThemeColors(SolidColorBrush brush)
    {
        // Text
        ColorTextType.Foreground = brush;
        ColorText.Foreground = brush;

        // Icons
        DropdownButtonIcon.Foreground = brush;
        CopyButtonIcon.Foreground = brush;
        IsEnabledIcon.Foreground = brush;

        // Message
        if (_isMessageOpen)
            Message.Foreground = brush;

        // Crosshair
        CrosshairHorizontal.Stroke = brush;
        CrosshairVertical.Stroke = brush;

        // Slider
        if (_slider1 != null) _slider1.Background = brush;
        if (_slider2 != null) _slider2.Background = brush;
        if (_slider3 != null) _slider3.Background = brush;

        // Slider text %
        ZoomLevelText.Foreground = brush;
    }

    private void UpdateMessageColor(SolidColorBrush brush)
    {   
        if (_isMessageOpen)
            Message.Foreground = brush;
    }

    private void UpdateZoomView(POINT p, int zoom)
    {   
        var invZoom = Math.Clamp(100 - zoom, 1, 100);
        ZoomImage.Source = CaptureRegion(p.X, p.Y, invZoom, invZoom);
    }

    private string HEX(byte r, byte g, byte b)
    {
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private string RGB(byte r, byte g, byte b)
    {
        return $"{r},{g},{b}";
    }

    private string HSV(byte r, byte g, byte b)
    {
        var (h, s, v) = ConvertToHSV(r, g, b);
        return $"{h:F0}°,{s * 100:F0}%,{v * 100:F0}%";
    }

    private string HSL(byte r, byte g, byte b)
    {
        var (h, s, l) = ConvertToHSL(r, g, b);
        return $"{h:F0}°,{s * 100:F0}%,{l * 100:F0}%";
    }

    private string CMYK(byte r, byte g, byte b)
    {
        var (c, m, y, k) = ConvertToCMYK(r, g, b);
        return $"{c * 100:F0}%,{m * 100:F0}%,{y * 100:F0}%,{k * 100:F0}%";
    }

    public async Task CopyColorToClipboard()
    {
        if (string.IsNullOrEmpty(CurrentTextContent)) return;

        try
        {
            Clipboard.SetText(CurrentTextContent);
        }
        catch
        {
            await ShowMessageAsync("Error!", 2000);
            return;
        }

        await ShowMessageAsync("Copied!", 3000);
    }

    private static (double H, double S, double L) ConvertToHSL(byte r, byte g, byte b)
    {
        double rNorm = r / 255.0;
        double gNorm = g / 255.0;
        double bNorm = b / 255.0;

        double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
        double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
        double delta = max - min;

        double h = 0;
        double s = 0;
        double l = (max + min) / 2.0;

        if (delta != 0)
        {
            s = delta / (1 - Math.Abs(2 * l - 1));

            if (max == rNorm)
                h = 60 * (((gNorm - bNorm) / delta) % 6);
            else if (max == gNorm)
                h = 60 * (((bNorm - rNorm) / delta) + 2);
            else // max == bNorm
                h = 60 * (((rNorm - gNorm) / delta) + 4);
        }

        if (h < 0) h += 360;

        return (h, s, l);
    }

    private static (double H, double S, double V) ConvertToHSV(byte r, byte g, byte b)
    {
        double rNorm = r / 255.0;
        double gNorm = g / 255.0;
        double bNorm = b / 255.0;

        double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
        double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
        double delta = max - min;

        double h = 0;
        if (delta != 0)
        {
            if (max == rNorm)
                h = 60 * (((gNorm - bNorm) / delta) % 6);
            else if (max == gNorm)
                h = 60 * (((bNorm - rNorm) / delta) + 2);
            else // max == bNorm
                h = 60 * (((rNorm - gNorm) / delta) + 4);
        }

        if (h < 0) h += 360;

        double s = (max == 0) ? 0 : delta / max;
        double v = max;

        return (h, s, v);
    }

    private static (double C, double M, double Y, double K) ConvertToCMYK(byte r, byte g, byte b)
    {
        double rNorm = r / 255.0;
        double gNorm = g / 255.0;
        double bNorm = b / 255.0;

        double k = 1 - Math.Max(rNorm, Math.Max(gNorm, bNorm));
        if (k >= 1.0 - 1e-6) // Black
            return (0, 0, 0, 1);

        double c = (1 - rNorm - k) / (1 - k);
        double m = (1 - gNorm - k) / (1 - k);
        double y = (1 - bNorm - k) / (1 - k);

        return (c, m, y, k);
    }

    public string GetColorType()
    { 
        return CurrentColorType.ToString();
    }
}
