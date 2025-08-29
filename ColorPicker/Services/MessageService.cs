using System.Windows;
using System.Windows.Media.Animation;


namespace to_lazy_to_curl.Services;

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
