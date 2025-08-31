using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ColorPicker.Services;

namespace ColorPicker.Components;

public partial class Settings : UserControl
{
    private const string NoModifierText = "Please use Ctrl, Alt, or Shift + a key";

    public Settings()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void Init() => KeybindInput.Text = State.GlobalKeybind;
    public void Reset() => ClearFocusFromFields();

    private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (KeybindInput.IsFocused) ClearFocusFromFields();
    }

    private void KeybindInput_GotFocus(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Got");
    }

    private void KeybindInput_LostFocus(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Lost");
        Init();
    }

    private void KeybindInput_KeyDown(object sender, KeyEventArgs e)
    {   
        Console.WriteLine("KeyDown Settings");

        e.Handled = true;
        var modifierKey = Keyboard.Modifiers;
        var key = (e.Key == Key.System) ? e.SystemKey : e.Key;

        // Cancel
        if (key == Key.Escape)
        {
            ClearFocusFromFields();
            Init();
            return;
        }

        // Ctrl + ...
        if (GlobalHotkeyManager.IsModifierKey(key))
        {
            KeybindInput.Text = $"{GlobalHotkeyManager.GetModifierKey(modifierKey)} + ";
            return;
        }

        // Require modifier for global hotkeys
        if (modifierKey == ModifierKeys.None)
        {
            KeybindInput.Text = NoModifierText;
            return;
        }

        // Set hotkey
        var hotkey = GlobalHotkeyManager.BuildHotkeyString(modifierKey, key);
        KeybindInput.Text = hotkey;

        State.GlobalKeybind = hotkey;
        // Save
    }

    private void ClearFocusFromFields()
    {   
        Console.WriteLine("UnFocusFields");
        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(KeybindInput), null);
        Keyboard.ClearFocus();
    }

    



    




}

