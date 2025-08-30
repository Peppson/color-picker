using System.Windows;
using System.Windows.Media.Animation;

namespace ColorPicker.Services;

static public class MessageService
{
    private static CancellationTokenSource? _messageCts;




    


    
    public static void ShowMessageBox(string msg)
    {
        MessageBox.Show(
            msg,
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
}
