using AigioL.Common.Net.ReverseProxy.Internals.Binary;
using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Internals.Configuration;

/// <summary>
/// 反向代理配置接口
/// </summary>
interface IReverseProxyConfig
{
    /// <summary>
    /// 获取或设置是否 Server 应在每个响应中包含标头
    /// </summary>
    bool AddServerHeader { get; set; }

    /// <summary>
    /// 获取或设置二级 Web 代理
    /// </summary>
    IWebProxy SecondLevelWebProxy { get; set; }

    /// <summary>
    /// 尝试获取脚本内容
    /// </summary>
    bool TryGetScriptContent(int localScriptId, out BinaryContent content);

    /// <summary>
    /// 获取所有域名表达式
    /// </summary>
    DomainPattern[] GetDomainPatterns();
}
