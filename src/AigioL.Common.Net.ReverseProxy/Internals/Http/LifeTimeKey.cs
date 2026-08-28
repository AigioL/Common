using AigioL.Common.Net.ReverseProxy.Internals.Configuration;

namespace AigioL.Common.Net.ReverseProxy.Internals.Http;

/// <summary>
/// 将域名用作 <see cref="KeyValuePair{TKey, TValue}.Key"/> 的生命周期记录
/// </summary>
sealed record class LifeTimeKey
{
    /// <summary>
    /// 所属域名
    /// </summary>
    public required string Domain { get; init; }

    /// <summary>
    /// 域名配置
    /// </summary>
    public required IDomainConfig DomainConfig { get; init; }
}