#if WINDOWS
using System.Text.Json.Serialization;

namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键注册结果
/// </summary>
public sealed record class HotkeyRegistrationResult
{
    /// <summary>
    /// 是否注册成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 注册的热键组合
    /// </summary>
    [JsonIgnore]
    public HotkeyCombo Combo { get; set; }

    public uint Modifiers => (uint)Combo.Modifiers;

    public uint Key => (uint)Combo.Key;

    public string? ComboString => Combo.ToString();

    /// <summary>
    /// Win32 错误代码
    /// </summary>
    public int NativeErrorCode { get; set; }

    public uint P0 { get; set; }

    public uint P1 { get; set; }

    public uint P2 { get; set; }
}
#endif