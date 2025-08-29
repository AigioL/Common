using MemoryPack;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.ApiGateway;

/// <summary>
/// 服务器证书（HTTPS/TLS 证书）验证键
/// </summary>
[MemoryPackable(SerializeLayout.Explicit)]
public readonly partial record struct ServerCertificateValidateKey
{
    /// <summary>
    /// Key = ServerCertificateValidateKey, Value = string 的字典的缓存键
    /// </summary>
    public const string CacheKey = nameof(ServerCertificateValidateKey);

    /// <summary>
    /// 匹配域名地址
    /// </summary>
    [MemoryPackOrder(0)]
    public string? MatchDomainName { get; init; }

    /// <summary>
    /// 凭证
    /// </summary>
    [MemoryPackOrder(1)]
    public required string Token { get; init; }
}