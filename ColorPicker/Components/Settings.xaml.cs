using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ColorPicker.Services;

namespace ColorPicker.Components;

public partial class Settings : UserControl
{
    private const string NoModifierText = "Missing modifier";

    public Settings()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }

    public void Init() => KeybindInput.Text = State.GlobalHotkey;
    public void Reset() => ClearFocus();

    private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (KeybindInput.IsFocused) ClearFocus(true);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        // Same as closing settings from titlebar
        State.MainWindow.TopTitleBar.SettingsButton_Click(sender, e);
    }

    private void KeybindInput_GotFocus(object sender, RoutedEventArgs e)
    {
        GlobalHotkeyManager.UnRegister(State.MainWindow);
    }

    private void KeybindInput_LostFocus(object sender, RoutedEventArgs e)
    {
        GlobalHotkeyManager.Register(State.MainWindow); // todo dynamic
        Init();
    }

    private void KeybindInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var modifierKey = Keyboard.Modifiers;
        var key = (e.Key == Key.System) ? e.SystemKey : e.Key;        
        e.Handled = true;

        // Cancel
        if (key == Key.Escape)
        {
            ClearFocus(true);
            Init();
            return;
        }

        // Ctrl + ...
        if (GlobalHotkeyManager.IsModifierKey(key))
        {
            KeybindInput.Text = $"{GlobalHotkeyManager.GetModifierKey(modifierKey)} + ";
            return;
        }

        // Require modifier
        if (modifierKey == ModifierKeys.None)
        {
            KeybindInput.Text = NoModifierText;
            return;
        }

        // Set hotkey
        var hotkey = GlobalHotkeyManager.BuildHotkeyString(modifierKey, key);
        State.GlobalHotkey = hotkey;
        Init();

        //todo Save hotkey 
    }

    private void ClearFocus(bool clearKeyboardFocus = false)
    {   
        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(KeybindInput), null);
        if (clearKeyboardFocus) Keyboard.ClearFocus();
    }
}
