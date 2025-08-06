#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 热键注册错误类型
/// </summary>
public enum HotkeyRegistrationError : byte
{
    /// <summary>
    /// 无错误
    /// </summary>
    None,

    /// <summary>
    /// 已被其他应用程序注册
    /// </summary>
    AlreadyRegisteredByOtherApp,

    /// <summary>
    /// 已被本应用程序注册
    /// </summary>
    AlreadyRegisteredByThisApp,

    /// <summary>
    /// 无效的窗口句柄
    /// </summary>
    InvalidWindowHandle,

    /// <summary>
    /// 无效的参数
    /// </summary>
    InvalidParameter,

    /// <summary>
    /// 未知错误
    /// </summary>
    Unknown,
}
#endif