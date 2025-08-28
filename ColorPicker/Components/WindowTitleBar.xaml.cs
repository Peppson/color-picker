using System.Windows;
using System.Windows.Controls;
using ColorPicker.Services;

namespace ColorPicker.Components;

public partial class WindowTitleBar : UserControl
{
    private ColorPicker _colorPicker = null!;
    private Window _mainWindow = null!;

    public WindowTitleBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {   
        _mainWindow = Window.GetWindow(this)!;
        _mainWindow.Topmost = true;

        // Grab ref to the ColorPicker
        if (_mainWindow is MainWindow main) _colorPicker = main.ColorPicker;
    }

    private void OnTopButton_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.Topmost = !_mainWindow.Topmost;
        DB.Print($"Window OnTop: {_mainWindow.Topmost}");

        this.OnTopButtonIcon.Foreground = _mainWindow.Topmost ? // todo color?
            (System.Windows.Media.Brush)Application.Current.Resources["PrimaryText"] : 
            (System.Windows.Media.Brush)Application.Current.Resources["OnTopDisabled"];
    }
   
    private void OnMinimizeButton_Click(object sender, RoutedEventArgs e) =>
        _mainWindow.WindowState = WindowState.Minimized;
	
	private void OnCloseButton_Click(object sender, RoutedEventArgs e) => 
        _mainWindow.Close();

    private void DebugButton_Click(object sender, RoutedEventArgs e) => 
        _colorPicker?.ToggleSampling();
}
