﻿using System.ComponentModel;
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
        ColorService.Init(this);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;        
    }

    private void OnNewFrame(object sender, EventArgs e)
    {
        // Clamp max fps, WPF framerates is wonky sometimes...
        const int minInterval = 1000 / Config.MaxSamplesPerSecond;

        if (!State.IsEnabled || State.IsMinimized) return;

        if (DateTime.UtcNow < _lastUpdate.AddMilliseconds(minInterval))
            return;
        _lastUpdate = DateTime.UtcNow;

        if (!Win32Api.GetCursorPos(out POINT p))
            return;

        if (_lastMousePos.X == p.X && _lastMousePos.Y == p.Y)
            return;
        _lastMousePos = p;

        if (!State.CaptureOnSelf && State.MainWindowPos.Contains(p.X, p.Y))
            return;

        UpdateColors(p);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        CurrentColorType = State.CurrentColorType;
        EnableInput();

        RegisterSliderParts();
        SetIsEnabledIcon(State.IsEnabled);
        UpdateColorsStatic();
        Console.WriteLine($"Loaded color type: {CurrentColorType}");
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        State.MainWindow.PreviewKeyDown -= ColorPicker_Keyboard_Click;
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
        _ = ColorService.CopyColorToClipboard();
        ColorService.UpdateMessageColor(_invertedBrush);
        e.Handled = true;
    }

    private void ToggleEnabled_Click(object sender, MouseButtonEventArgs e)
    {
        ToggleIsEnabled();
        e.Handled = true;
    }

    private void ColorPicker_Keyboard_Click(object sender, KeyEventArgs e)
    {   
        Console.WriteLine("KeyDown ColorPicker"); // todo


        // CTRL + C
        if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            _ = ColorService.CopyColorToClipboard();
            ColorService.UpdateMessageColor(_invertedBrush);
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
            ZoomLevel = Math.Min(ZoomLevel + step, (int)Config.MaxZoomLevel);
        else
            ZoomLevel = Math.Max(ZoomLevel - step, (int)Config.MinZoomLevel);

        e.Handled = true;
    }

    public void ToggleIsEnabled()
    {
        State.IsEnabled = !State.IsEnabled;
        SetIsEnabledIcon(State.IsEnabled);
    }

    public void SetIsEnabled(bool enabled)
    {
        State.IsEnabled = enabled;
        SetIsEnabledIcon(State.IsEnabled);
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

    private void UpdateZoomView(POINT p, int zoom)
    {
        var invertedZoom = Math.Clamp(100 - zoom, 1, 100);
        ZoomView.Source = ScreenCaptureService.GetRegion(p.X, p.Y, invertedZoom, invertedZoom);
    }

    private void UpdateColors(POINT p)
    {
        byte r, g, b;
        (r, g, b) = ColorService.GetColorAtPos(p);

        // ZoomView
        UpdateZoomView(p, _zoomLevel);

        if (ColorService.IsSameColor(_currentBrush, r, g, b)) return;

        _currentBrush.Color = Color.FromRgb(r, g, b);
        _invertedBrush.Color = ColorService.GetInvertedColor(r, g, b);

        // UI
        ColorService.UpdatePreviewView(_currentBrush);
        ColorService.UpdateTextContent(r, g, b, CurrentColorType);
        ColorService.UpdateThemeColors(_invertedBrush);
    }

    private void UpdateColorsStatic()
    {
        byte r = _currentBrush.Color.R;
        byte g = _currentBrush.Color.G;
        byte b = _currentBrush.Color.B;

        ColorService.UpdatePreviewView(_currentBrush);
        ColorService.UpdateTextContent(r, g, b, CurrentColorType);
        ColorService.UpdateThemeColors(_invertedBrush);
    }

    public string GetColorType()
    {
        return CurrentColorType.ToString();
    }

    public void EnableInput()
    {
        State.MainWindow.PreviewKeyDown += ColorPicker_Keyboard_Click;
        CompositionTarget.Rendering += OnNewFrame!;

        ZoomView.MouseWheel += ZoomView_MouseWheel;
        ZoomView.MouseDown += ZoomView_MouseDown;
        ZoomView.MouseMove += ZoomView_MouseMove;
        ZoomView.MouseUp += ZoomView_MouseUp; // todo new window insterad? 
    }

    public void DisableInput()
    {
        State.MainWindow.PreviewKeyDown -= ColorPicker_Keyboard_Click;
        CompositionTarget.Rendering -= OnNewFrame!;

        ZoomView.MouseWheel -= ZoomView_MouseWheel;
        ZoomView.MouseDown -= ZoomView_MouseDown;
        ZoomView.MouseMove -= ZoomView_MouseMove;
        ZoomView.MouseUp -= ZoomView_MouseUp;
    }

    private void RegisterSliderParts()
    {
        ZoomSlider.ApplyTemplate();

        if (ZoomSlider.Template.FindName("PART_Track", ZoomSlider) is Track track)
        {
            track.DecreaseRepeatButton.ApplyTemplate();
            track.IncreaseRepeatButton.ApplyTemplate();
            track.Thumb.ApplyTemplate();

            Slider_2 = track.DecreaseRepeatButton as RepeatButton;
            Slider_3 = track.IncreaseRepeatButton as RepeatButton;

            if (track.Thumb.Template.FindName("PART_ThumbBorder", track.Thumb) is Border thumbBorder)
            {
                Slider_1 = thumbBorder;
            }
        }
    }
}
