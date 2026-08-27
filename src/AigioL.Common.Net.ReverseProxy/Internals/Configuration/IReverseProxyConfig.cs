using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Internals.Configuration;

/// <summary>
/// 反向代理配置接口
/// </summary>
interface IReverseProxyConfig
{
    /// <summary>
    /// 获取所有域名表达式
    /// </summary>
    DomainPattern[] GetDomainPatterns();
}
