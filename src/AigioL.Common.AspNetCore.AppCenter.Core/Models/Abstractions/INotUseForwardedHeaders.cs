using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

/// <summary>
/// 配置是否使用 nginx 反向代理
/// </summary>
public partial interface INotUseForwardedHeaders
{
    /// <summary>
    /// 不启用反向代理
    /// </summary>
    bool NotUseForwardedHeaders { get; }

    /// <summary>
    /// 配置 <see cref="ForwardedHeadersOptions.KnownProxies"/>，使用英文分号分隔
    /// </summary>
    string? ForwardedHeadersKnownProxies { get; }

    /// <summary>
    /// 获取用于 <see cref="ForwardedHeadersOptions.KnownProxies"/> 的 IP 地址列表
    /// </summary>
    /// <returns></returns>
    IPAddress[] GetForwardedHeadersKnownProxies() => GetForwardedHeadersKnownProxies(ForwardedHeadersKnownProxies);

    static IPAddress[] GetForwardedHeadersKnownProxies(string? knownProxies)
    {
        if (string.IsNullOrWhiteSpace(knownProxies))
        {
            return [IPAddress.Parse("::ffff:172.18.0.1")];
        }
        else
        {
            return [.. knownProxies.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(static x => IPAddress2.TryParse(x, out var v) ? v : null!)
                .Where(static x => x != null)];
        }
    }
}