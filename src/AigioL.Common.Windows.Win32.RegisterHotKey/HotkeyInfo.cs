#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键信息
/// </summary>
public sealed record HotkeyInfo(int Id, ModifierKeys Modifiers, VirtualKey Key, Action? Action);
#endif