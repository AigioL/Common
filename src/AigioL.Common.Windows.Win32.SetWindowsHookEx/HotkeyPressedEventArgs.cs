#if WINDOWS
using System.Text.Json.Serialization;

namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键按下事件参数
/// </summary>
public sealed class HotkeyPressedEventArgs : EventArgs
{
    public uint Modifiers => (uint)Combo.Modifiers;

    public uint Key => (uint)Combo.Key;

    public string ComboString => Combo.ToString();

    [JsonIgnore]
    public HotkeyCombo Combo { get; init; }

    public uint P0 { get; init; }

    public uint P1 { get; init; }

    public uint P2 { get; init; }
}
#endif