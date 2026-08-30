namespace AigioL.Common.Net.ReverseProxy.Models;

/// <summary>
/// 启动 <see cref="IReverseProxyWebApp"/> 的错误代码
/// </summary>
public enum ReverseProxyWebAppStartErrCode : byte
{
    /// <summary>
    /// 启动成功
    /// </summary>
    Ok = 0,

    /// <summary>
    /// 早期 Windows 版本且缺少 Windows 更新补丁，Icu 存在 Bug 导致，要么更新操作系统，要么更改配置使用 NLS 重启程序，也可以携带和使用其自己的 ICU 副本修复
    /// <para>通常问题出在 1703 ~ 1903 之间的 Windows 10 版本</para>
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/core/extensions/globalization-icu#use-nls-instead-of-icu</para>
    /// </summary>
    IcuTestFail = 1,
}
