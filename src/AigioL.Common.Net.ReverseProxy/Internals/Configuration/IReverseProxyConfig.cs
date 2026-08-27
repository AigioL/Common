using AigioL.Common.Net.ReverseProxy.Internals.Binary;
using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Internals.Configuration;

/// <summary>
/// 反向代理配置接口
/// </summary>
interface IReverseProxyConfig
{
    /// <summary>
    /// 尝试获取脚本内容
    /// </summary>
    bool TryGetScriptContent(int localScriptId, out BinaryContent content);

    /// <summary>
    /// 获取所有域名表达式
    /// </summary>
    DomainPattern[] GetDomainPatterns();
}
