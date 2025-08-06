#if WINDOWS
namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 修饰键枚举
/// </summary>
[Flags]
public enum ModifierKeys : uint
{
    /// <summary>
    /// 无修饰键
    /// </summary>
    None = 0,

    /// <summary>
    /// Alt 键
    /// </summary>
    Alt = 0x0001,

    /// <summary>
    /// Control 键
    /// </summary>
    Control = 0x0002,

    /// <summary>
    /// Shift 键
    /// </summary>
    Shift = 0x0004,

    /// <summary>
    /// Windows 键
    /// </summary>
    Win = 0x0008,
}
#endif