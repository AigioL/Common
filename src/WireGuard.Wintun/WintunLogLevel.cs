namespace WireGuard.Wintun;

/// <summary>
/// 确定日志记录级别，并将其传递给 <see cref="WintunLogCallback"/>
/// </summary>
public enum WintunLogLevel
{
    /// <summary>
    /// Informational
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning
    /// </summary>
    Warn = 1,

    /// <summary>
    /// Error
    /// </summary>
    Error = 2,
}
