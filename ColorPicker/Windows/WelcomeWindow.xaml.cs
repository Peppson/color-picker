using System.Windows;
using ColorPicker.Services;

namespace ColorPicker.Windows;

public partial class WelcomeWindow : Window
{
    public WelcomeWindow()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {   
        State.IsFirstBoot = false;
        this.Close();
    }
}
