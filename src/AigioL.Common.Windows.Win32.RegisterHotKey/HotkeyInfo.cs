#if WINDOWS
using System.Text.Json.Serialization;

namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键信息
/// </summary>
public sealed record HotkeyInfo(int Id, [property: JsonIgnore] HotkeyCombo Combo, [property: JsonIgnore] Action? Action, string? Description = null)
{
    public uint Modifiers => (uint)Combo.Modifiers;

    public uint Key => (uint)Combo.Key;

    public string ComboString => Combo.ToString();
}
#endif