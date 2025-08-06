#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键按下事件参数
/// </summary>
public sealed class HotkeyPressedEventArgs : EventArgs
{
    public HotkeyInfo HotkeyInfo { get; }

    public HotkeyPressedEventArgs(HotkeyInfo hotkeyInfo)
    {
        HotkeyInfo = hotkeyInfo;
    }
}
#endif