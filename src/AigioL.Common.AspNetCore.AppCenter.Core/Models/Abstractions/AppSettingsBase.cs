using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.JsonWebTokens.Models.Abstractions;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

public abstract partial class AppSettingsBase : JsonWebTokenOptions
{
    /// <summary>
    /// 控制 Job 是否不启用的开关，值为 Job 的名称
    /// </summary>
    public string[]? CloseFunctions { get; set; }
}

partial class AppSettingsBase : INotUseForwardedHeaders, IViewsUrl
{
    /// <inheritdoc/>
    public bool NotUseForwardedHeaders { get; set; }

    /// <inheritdoc/>
    public string? ForwardedHeadersKnownProxies { get; set; }

    /// <inheritdoc/>
    public virtual IPAddress[] GetForwardedHeadersKnownProxies() => INotUseForwardedHeaders.GetForwardedHeadersKnownProxies(ForwardedHeadersKnownProxies);

    /// <inheritdoc/>
    public string? ViewsUrl { get; set; }

    /// <inheritdoc/>
    public virtual string[] GetOrigins() => IViewsUrl.GetOrigins(ViewsUrl);

    /// <summary>
    /// 供 <see cref="IDataProtectionService"/> 服务使用的密钥
    /// </summary>
    public string? DataProtectionKey { get; set; }
}
