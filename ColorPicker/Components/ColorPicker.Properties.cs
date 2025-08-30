using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ColorPicker.Models;
using ColorPicker.Settings;

namespace ColorPicker.Components;

public partial class ColorPicker : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Props
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


    // Fields
    private bool _isDragging = false;
    private POINT _dragStartMouse;
    private POINT _dragStartPos;
    private DateTime _lastUpdate = DateTime.UtcNow;
    private POINT _lastMousePos;
    private SolidColorBrush _currentBrush = new(Colors.White);
    private SolidColorBrush _invertedBrush = new(Colors.Black);
    private Border? _slider1;
    private RepeatButton? _slider2;
    private RepeatButton? _slider3;
}
