#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键注册失败事件参数
/// </summary>
public sealed class HotkeyRegistrationFailedEventArgs : EventArgs
{
    public HotkeyCombo Combo { get; }

    public int NativeErrorCode { get; }

    public HotkeyRegistrationError ErrorCode { get; }

    public string Message { get; }

    public HotkeyRegistrationFailedEventArgs(HotkeyCombo combo, int nativeErrorCode, HotkeyRegistrationError errorCode, string message)
    {
        Combo = combo;
        NativeErrorCode = nativeErrorCode;
        ErrorCode = errorCode;
        Message = message;
    }
}
#endif