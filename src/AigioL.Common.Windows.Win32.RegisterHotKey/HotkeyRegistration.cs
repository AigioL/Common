#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键注册记录
/// </summary>
public sealed record HotkeyRegistration(
    int Id,
    HotkeyCombo Combo,
    Action? Action,
    string? Description);
#endif