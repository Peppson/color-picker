using System.Windows;

namespace ColorPicker;

public partial class MainWindow : Window
{
    public MainWindow()
    {   
        InitializeComponent();
        //SetWindowSizeAndPosition();
    }

    /* protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		TopTitleBar.InitializeHook();
	} */

    /* private void SetWindowSizeAndPosition()
	{
		if (Properties.Settings.Default.WindowWidth <= 0) return;

		Width = Properties.Settings.Default.WindowWidth;
		Height = Properties.Settings.Default.WindowHeight;
		Top = Properties.Settings.Default.WindowTop;
		Left = Properties.Settings.Default.WindowLeft;
	} */

    /* protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
	{
		base.OnClosing(e);
		LogService.Shutdown();

		Properties.Settings.Default.WindowWidth = Width;
		Properties.Settings.Default.WindowHeight = Height;
		Properties.Settings.Default.WindowTop = Top;
		Properties.Settings.Default.WindowLeft = Left;

		Properties.Settings.Default.UrlInputText = AppState.UrlInput.GetUrlText();
		Properties.Settings.Default.PayloadText = AppState.EditorInput.GetPayloadText();
		Properties.Settings.Default.ResponseText = AppState.EditorInput.GetResponseText();
		Properties.Settings.Default.HeaderText = AppState.EditorInput.GetHeaderText();
		Properties.Settings.Default.IsDarkTheme = ThemeService.GetIsDarkTheme();

		Properties.Settings.Default.Save();
	} */

    
}
