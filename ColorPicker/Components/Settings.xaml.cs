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

    public void RefreshHotkeyInput() => KeybindInput.Text = State.GlobalHotkey;
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
        if (!string.IsNullOrWhiteSpace(State.GlobalHotkey))
            GlobalHotkeyManager.UnRegister(State.MainWindow);
    }

    private void KeybindInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(State.GlobalHotkey))
            _ = GlobalHotkeyManager.Register(State.MainWindow, State.GlobalHotkey);

        RefreshHotkeyInput();
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
            RefreshHotkeyInput();
            return;
        }

        // Require at least one modifier
        if (modifierKey == ModifierKeys.None)
        {
            KeybindInput.Text = NoModifierText;
            return;
        }

        // Ctrl + Alt + ...
        if (GlobalHotkeyManager.IsModifierKey(key))
        {
            KeybindInput.Text = $"{GlobalHotkeyManager.BuildModifiersString(modifierKey)}+";
            return;
        }

        // Same hotkey
        var hotkey = GlobalHotkeyManager.BuildHotkeyString(modifierKey, key);
        if (hotkey == State.GlobalHotkey)
        { 
            RefreshHotkeyInput();
            return;
        }
        
        // Set new hotkey
        if (!GlobalHotkeyManager.Register(State.MainWindow, hotkey))
        {
            MessageService.ShowMessageBox("Failed to register hotkey. It might already be in use by another application.");
            RefreshHotkeyInput();
            return;
        }
        
        State.GlobalHotkey = hotkey;
        RefreshHotkeyInput();
    }

    private void ClearFocus(bool clearKeyboardFocus = false)
    {   
        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(KeybindInput), null);
        if (clearKeyboardFocus) Keyboard.ClearFocus();
    }
}
