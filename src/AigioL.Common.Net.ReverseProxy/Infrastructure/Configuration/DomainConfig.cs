using Microsoft.AspNetCore.Http;
using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;

/// <summary>
/// <see cref="IDomainConfig"/> 的默认实现
/// </summary>
public sealed partial record class DomainConfig : IDomainConfig
{
    /// <inheritdoc/>
    public bool TlsSni { get; set; }

    /// <inheritdoc/>
    public string? TlsSniPattern { get; set; }

    /// <inheritdoc/>
    public bool TlsIgnoreNameMismatch { get; set; }

    /// <inheritdoc/>
    public IPAddress? IPAddress { get; set; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; set; }

    /// <inheritdoc/>
    public Uri? Destination { get; set; }

    /// <inheritdoc/>
    public IResult? Response { get; set; }

    /// <inheritdoc/>
    public string? ForwardDestination { get; set; }

    /// <inheritdoc/>
    public string? UserAgent { get; set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<DomainPattern, IDomainConfig>? Items { get; set; }

    /// <inheritdoc/>
    public bool IsServerSideProxy { get; }

    public static readonly DomainConfig Default = new()
    {
        TlsSni = true,
    };
}
