#if WINDOWS
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
    /// 热键 Id（成功时有效）
    /// </summary>
    public int HotkeyId { get; set; }

    /// <summary>
    /// 注册的热键组合
    /// </summary>
    public HotkeyCombo? Combo { get; set; }

    /// <summary>
    /// Win32 错误代码
    /// </summary>
    public int NativeErrorCode { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    public HotkeyRegistrationError ErrorCode { get; set; }

    /// <summary>
    /// 结果消息
    /// </summary>
    public string? Message { get; set; }
}
#endif