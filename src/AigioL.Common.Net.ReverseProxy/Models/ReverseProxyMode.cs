namespace AigioL.Common.Net.ReverseProxy.Models;

/// <summary>
/// 反向代理模式（网络加速模式）
/// </summary>
public enum ReverseProxyMode : byte
{
    /// <summary>
    /// 配置系统代理设置实现代理流量
    /// </summary>
    System = 0,

    /// <summary>
    /// 修改 HOSTS 文件实现代理流量
    /// </summary>
    Hosts = 1,

    /// <summary>
    /// 使用 Wintun 创建虚拟网卡实现代理流量
    /// </summary>
    Wintun = 3,

    /// <summary>
    /// 仅开启端口监听的代理模式，适用于外部程序自行处理流量
    /// </summary>
    ProxyOnly = 4,

    /// <summary>
    /// 使用 PAC 文件实现代理流量
    /// </summary>
    Pac = 5,

    /// <summary>
    /// 使用 WinDivert 实现 DNS 拦截代理流量
    /// </summary>
    WinDivert = 6,
}
