namespace AigioL.Common.Net.NameResolution.Models;

/// <summary>
/// DNS 返回结果来源类型
/// </summary>
public enum DnsResultSourceType : byte
{
    /// <summary>
    /// 结果来自固定值，通常是错误结果
    /// </summary>
    FixedValue = 1,

    /// <summary>
    /// 结果来自系统默认 DNS 服务器
    /// </summary>
    SystemDefault = 2,

    /// <summary>
    /// 结果来自配置的 DNS 服务器组
    /// </summary>
    DnsServers = 3,

    /// <summary>
    /// 结果来自配置的 DoH 地址组
    /// </summary>
    DohServers = 4,
}
