#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

partial class GlobalHotkeyListener
{
    /// <summary>
    /// 热键注册失败事件
    /// </summary>
    public event EventHandler<HotkeyRegistrationFailedEventArgs>? HotkeyRegistrationFailed;

    /// <summary>
    /// 热键按下事件
    /// </summary>
    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;
}
#endif