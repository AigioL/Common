using AigioL.Common.JsonWebTokens.Models.Abstractions;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

public abstract partial class AppSettingsBase : JsonWebTokenOptions
{
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
}
