using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AigioL.Common.Net.ReverseProxy.Services.Abstractions;

/// <summary>
/// <see cref="X509Certificate"/> 证书服务接口
/// </summary>
public interface IX509CertService
{
    /// <summary>
    /// 根据 SSL 客户端 Hello 信息获取服务器证书
    /// </summary>
    ValueTask<X509Certificate> GetServerCertificateAsync(SslClientHelloInfo info, CancellationToken cancellationToken = default);
}
