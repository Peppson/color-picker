using System.Windows;
using System.Windows.Media.Animation;

namespace ColorPicker.Services;

static public class MessageService
{
    private static CancellationTokenSource? _messageCts;
    public static bool IsMessageOpen { get; private set; } = false;

    public static async Task ShowAsync(Components.ColorPicker colorPicker, string text, int durationMs)
    {
        IsMessageOpen = true;

        // Cancel any previous messages
        _messageCts?.Cancel();
        _messageCts = new CancellationTokenSource();
        var token = _messageCts.Token;

        colorPicker.Message.Text = text;

        // Fade in
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1));
        colorPicker.Message.BeginAnimation(UIElement.OpacityProperty, fadeIn);

        // Fade out
        try
        {
            await Task.Delay(durationMs, token);
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            colorPicker.Message.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        catch (TaskCanceledException) { }

        IsMessageOpen = false;
    }

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
