using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.JsonWebTokens.Models.Abstractions;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 微服务的配置项，使用 UserSecrets 存储值
/// <para>https://learn.microsoft.com/zh-cn/aspnet/core/security/app-secrets</para>
/// </summary>
public partial class MSAppSettings : JsonWebTokenOptions
{
}

partial class MSAppSettings : INotUseForwardedHeaders
{
    /// <inheritdoc/>
    public bool NotUseForwardedHeaders { get; set; }

    /// <inheritdoc/>
    public string? ForwardedHeadersKnownProxies { get; set; }

    /// <inheritdoc/>
    public virtual IPAddress[] GetForwardedHeadersKnownProxies() => INotUseForwardedHeaders.GetForwardedHeadersKnownProxies(ForwardedHeadersKnownProxies);
}
