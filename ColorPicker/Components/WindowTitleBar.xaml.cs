using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ColorPicker.Components;

public partial class WindowTitleBar : UserControl
{
    private const int WM_GETMINMAXINFO = 0x0024;
	private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
    private Window _parentWindow = null!;

	[DllImport("user32.dll")]
	private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

	[DllImport("user32.dll")]
	private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT(int left, int top, int right, int bottom)
    {
		public int Left = left;
		public int Top = top;
		public int Right = right;
		public int Bottom = bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
	public struct MONITORINFO
	{
		public int cbSize;
		public RECT rcMonitor;
		public RECT rcWork;
		public uint dwFlags;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT(int x, int y)
    {
		public int X = x;
		public int Y = y;
    }

    [StructLayout(LayoutKind.Sequential)]
	public struct MINMAXINFO
	{
		public POINT ptReserved;
		public POINT ptMaxSize;
		public POINT ptMaxPosition;
		public POINT ptMinTrackSize;
		public POINT ptMaxTrackSize;
	}

    public WindowTitleBar()
    {
        InitializeComponent();
        this.Loaded += (s, e) =>
        {
            _parentWindow = Window.GetWindow(this);
        };
    }

    private void OnTopButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("OnTop button clicked");
    }
   
    private void OnMinimizeButton_Click(object sender, RoutedEventArgs e) =>
        _parentWindow.WindowState = WindowState.Minimized;
	
	private void OnCloseButton_Click(object sender, RoutedEventArgs e) => 
        _parentWindow.Close();

    public void InitializeHook()
    {
        if (_parentWindow != null)
        {
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }
    }
    
	public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_GETMINMAXINFO)
        {
            MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                GetMonitorInfo(monitor, ref monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        return IntPtr.Zero;
    }
}
