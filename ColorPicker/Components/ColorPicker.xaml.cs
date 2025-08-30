using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ColorPicker.Models;
using ColorPicker.Services;
using ColorPicker.Settings;
using FontAwesome.WPF;

namespace ColorPicker.Components;

public partial class ColorPicker : UserControl, INotifyPropertyChanged
{    
    public ColorPicker()
    {
        InitializeComponent();
        DataContext = this;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        CompositionTarget.Rendering += OnNewFrame!;

        ZoomView.MouseWheel += ZoomView_MouseWheel;
        ZoomView.MouseDown += ZoomView_MouseDown;
        ZoomView.MouseMove += ZoomView_MouseMove;
        ZoomView.MouseUp += ZoomView_MouseUp;
    }

    private void OnNewFrame(object sender, EventArgs e)
    {
        // Clamp to max 120fps color updates, WPF framerates is wonky sometimes...
        const int minInterval = 1000 / 120;

        if (!State.IsEnabled || State.IsMinimized)
            return;

        if (DateTime.UtcNow < _lastUpdate.AddMilliseconds(minInterval))
            return;
        _lastUpdate = DateTime.UtcNow;

        if (!Win32Api.GetCursorPos(out POINT p))
            return;

        if (!State.CaptureOnSelf && State.MainWindowPos.Contains(p.X, p.Y))
            return;

        if (_lastMousePos.X == p.X && _lastMousePos.Y == p.Y)
            return;
        _lastMousePos = p;

        UpdateColors(p);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        State.MainWindow.PreviewKeyDown += Keyboard_Click;

        CurrentColorType = ColorService.StringToColorType(Properties.Settings.Default.ColorType); // todo

        if (AppConfig.IsEnabledOverride != null)
            State.IsEnabled = AppConfig.IsEnabledOverride.Value;

        SetIsEnabledIcon(State.IsEnabled);
        RegisterSliderParts();
        UpdateColorsStatic();
        Console.WriteLine($"Loaded color type: {CurrentColorType}");
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        State.MainWindow.PreviewKeyDown -= Keyboard_Click;
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
            UpdateColorsStatic();
        }
        e.Handled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        //_ = CopyColorToClipboard();
        Console.WriteLine("Copy color to clipboard (todo)");
        UpdateMessageColor(_invertedBrush);
        e.Handled = true;
    }

    private void ToggleEnabled_Click(object sender, MouseButtonEventArgs e)
    {
        ToggleIsEnabled();
        e.Handled = true;
    }

    private void Keyboard_Click(object sender, KeyEventArgs e)
    {
        // CTRL + C
        if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            //_ = CopyColorToClipboard(); todo
            Console.WriteLine("Copy color to clipboard (todo)");
            UpdateMessageColor(_invertedBrush);
            e.Handled = true;
            return;
        }

        // Spacebar 
        else if (e.Key == Key.Space)
        {
            ToggleIsEnabled();
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

    private void ZoomView_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        const int step = 2;

        if (e.Delta > 0)
            ZoomLevel = Math.Min(ZoomLevel + step, (int)AppConfig.MaxZoomLevel);
        else
            ZoomLevel = Math.Max(ZoomLevel - step, (int)AppConfig.MinZoomLevel);

        e.Handled = true;
    }

    public void ToggleIsEnabled()
    {
        State.IsEnabled = !State.IsEnabled;
        SetIsEnabledIcon(State.IsEnabled);
        Console.WriteLine($"Color sampling: {State.IsEnabled}");
    }

    public void SetIsEnabled(bool enabled)
    {
        State.IsEnabled = enabled;
        SetIsEnabledIcon(State.IsEnabled);
        Console.WriteLine($"Color sampling: {State.IsEnabled}");
    }

    private void SetIsEnabledIcon(bool running)
    {
        IsEnabledIcon.Icon = running ? FontAwesomeIcon.Pause : FontAwesomeIcon.Play;
    }

    private void ZoomView_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            Mouse.OverrideCursor = Cursors.None;

            if (Win32Api.GetCursorPos(out _dragStartMouse))
                _dragStartPos = _lastMousePos;

            ZoomView.CaptureMouse();
        }
    }

    private void ZoomView_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && Win32Api.GetCursorPos(out POINT currentMouse))
        {
            int dx = currentMouse.X - _dragStartMouse.X;
            int dy = currentMouse.Y - _dragStartMouse.Y;
            _lastMousePos.X = _dragStartPos.X + dx;
            _lastMousePos.Y = _dragStartPos.Y + dy;

            UpdateZoomView(_lastMousePos, ZoomLevel);
            UpdateColors(_lastMousePos);
        }
    }

    private void ZoomView_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ZoomView.ReleaseMouseCapture();

            // Set mouse pos back where we started
            Win32Api.SetCursorPos(_dragStartMouse.X, _dragStartMouse.Y);
            Mouse.OverrideCursor = null;
        }
    }
    
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
        IntPtr hdc = Win32Api.GetDC(IntPtr.Zero);
        uint colorRef = Win32Api.GetPixel(hdc, p.X, p.Y);
        _ = Win32Api.ReleaseDC(IntPtr.Zero, hdc);

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
                Console.WriteLine("Unknown color type");
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
        if (MessageService.IsMessageOpen)
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
        if (MessageService.IsMessageOpen)
            Message.Foreground = brush;
    }

    private void UpdateZoomView(POINT p, int zoom)
    {
        var invZoom = Math.Clamp(100 - zoom, 1, 100);
        ZoomView.Source = ScreenCaptureService.GetRegion(p.X, p.Y, invZoom, invZoom);
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
