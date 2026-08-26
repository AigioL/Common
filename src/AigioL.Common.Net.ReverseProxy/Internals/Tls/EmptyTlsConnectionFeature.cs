using Microsoft.AspNetCore.Http.Features;
using System.Security.Cryptography.X509Certificates;

namespace AigioL.Common.Net.ReverseProxy.Internals.Tls;

/// <summary>
/// 空的 <see cref="ITlsConnectionFeature"/> 实现，返回 <see langword="null"/> 证书
/// </summary>
sealed class EmptyTlsConnectionFeature : ITlsConnectionFeature
{
    /// <summary>
    /// 获取当前实例
    /// </summary>
    public static readonly EmptyTlsConnectionFeature Instance = new();

    EmptyTlsConnectionFeature() { }

    /// <inheritdoc/>
    public X509Certificate2? ClientCertificate
    {
        get => null;
        set { }
    }

    /// <inheritdoc/>
    public Task<X509Certificate2?> GetClientCertificateAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<X509Certificate2?>(null);
    }
}