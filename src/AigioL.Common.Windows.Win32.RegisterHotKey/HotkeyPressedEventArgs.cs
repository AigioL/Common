#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键按下事件参数
/// </summary>
public sealed class HotkeyPressedEventArgs : EventArgs
{
    public HotkeyInfo HotkeyInfo { get; }

    public string? Description { get; }

    public HotkeyPressedEventArgs(HotkeyInfo hotkeyInfo, string? description = null)
    {
        HotkeyInfo = hotkeyInfo;
        Description = description;
    }
}
#endif