using AigioL.Common.Net.ReverseProxy.Infrastructure.Binary;
using AigioL.Common.Net.ReverseProxy.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;

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
    /// 网络加速模式
    /// </summary>
    ReverseProxyMode ProxyMode { get; set; }

    /// <summary>
    /// 是否匹配指定的域名
    /// </summary>
    bool IsMatch(HostString host);

    /// <summary>
    /// 尝试获取脚本内容
    /// </summary>
    bool TryGetScriptContent(int localScriptId, out BinaryContent content);

    /// <summary>
    /// 尝试获取域名配置
    /// </summary>
    bool TryGetDomainConfig(HttpRequest req, [MaybeNullWhen(false)] out IDomainConfig value);

    /// <summary>
    /// 尝试获取脚本配置
    /// </summary>
    bool TryGetScriptConfig(HttpRequest req, [MaybeNullWhen(false)] out IReadOnlyCollection<IScriptConfig> value);

    /// <summary>
    /// 获取所有域名表达式
    /// </summary>
    DomainPattern[] GetDomainPatterns();

    /// <summary>
    /// 开启网络加速后仅代理脚本而不进行网络加速
    /// </summary>
    bool OnlyEnableProxyScript { get; set; }

    /// <summary>
    /// 启用 HTTP 链接转发到 HTTPS
    /// </summary>
    bool EnableHttpProxyToHttps { get; set; }

    /// <summary>
    /// 是否只针对 Steam 内置浏览器启用脚本
    /// </summary>
    bool IsOnlyWorkSteamBrowser { get; set; }
}
