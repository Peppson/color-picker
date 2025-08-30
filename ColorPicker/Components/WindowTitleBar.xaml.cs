using System.Windows;
using System.Windows.Controls;
using ColorPicker.Services;
using ColorPicker.Settings;

namespace ColorPicker.Components;

public partial class WindowTitleBar : UserControl
{
    public WindowTitleBar()
    {
        InitializeComponent();
        #if !RELEASE
            DebugButton.Visibility = Visibility.Visible; 
        #endif
    }

    private void OnTopButton_Click(object sender, RoutedEventArgs e)
    {
        State.MainWindow.Topmost = !State.MainWindow.Topmost;
        OnTopButtonIcon.Foreground = 
            ColorService.GetIconColor(State.MainWindow.Topmost);
    }
   
    private void OnMinimizeButton_Click(object sender, RoutedEventArgs e) =>
        State.MainWindow.WindowState = WindowState.Minimized;
	
	private void OnCloseButton_Click(object sender, RoutedEventArgs e) => 
        State.MainWindow.Close();

    private void DebugButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Debug Reset - AppState");
        State.Reset(); // todo
    }
}
