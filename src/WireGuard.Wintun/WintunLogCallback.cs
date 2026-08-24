using System.Runtime.InteropServices;

namespace WireGuard.Wintun;

/// <summary>
/// 由内部日志记录器调用以报告诊断消息
/// </summary>
/// <param name="level">消息级别</param>
/// <param name="timestamp">自 1601/1/1（UTC）以来的消息时间戳，间隔为 100 纳秒</param>
/// <param name="message">消息文本</param>
public delegate void WintunLogCallback(
    [MarshalAs(UnmanagedType.I4)] WintunLogLevel level,
    ulong timestamp,
    [MarshalAs(UnmanagedType.LPWStr)] string message
);