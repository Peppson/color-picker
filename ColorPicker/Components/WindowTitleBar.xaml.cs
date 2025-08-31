using System.Windows;
using System.Windows.Controls;
using ColorPicker.Services;
using ColorPicker.Settings;

namespace ColorPicker.Components;

#pragma warning disable CS0162

public partial class WindowTitleBar : UserControl
{
    public WindowTitleBar()
    {
        InitializeComponent();

        #if !RELEASE
            if (Config.ShowDebugbutton)
                DebugButton.Visibility = Visibility.Visible;
                
            if (Config.StartWithSettings)
                Loaded += SettingsButton_Click;
        #endif
    }

    private void OnTopButton_Click(object sender, RoutedEventArgs e)
    {
        State.MainWindow.Topmost = !State.MainWindow.Topmost;
        OnTopButtonIcon.Foreground =
            ColorService.GetIconColor(State.MainWindow.Topmost);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        State.IsSettingsOpen = !State.IsSettingsOpen;

        if (State.IsSettingsOpen)
        {
            State.MainWindow.ColorPicker.DisableInput();
            State.MainWindow.SettingsWindow.Init();
            State.MainWindow.ColorPicker.Visibility = Visibility.Collapsed;
            State.MainWindow.SettingsWindow.Visibility = Visibility.Visible;
        }
        else
        {
            State.MainWindow.ColorPicker.EnableInput();
            State.MainWindow.SettingsWindow.Reset();
            State.MainWindow.ColorPicker.Visibility = Visibility.Visible;
            State.MainWindow.SettingsWindow.Visibility = Visibility.Collapsed;
        }
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

#pragma warning restore CS0162
