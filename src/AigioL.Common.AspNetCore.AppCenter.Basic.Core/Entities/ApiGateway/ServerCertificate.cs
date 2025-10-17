using AigioL.Common.AspNetCore.AppCenter.Basic.Models.ApiGateway;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.ApiGateway;

/// <summary>
/// 服务器证书（HTTPS/TLS 证书）表实体类
/// </summary>
[Table("ServerCertificates")]
public partial class ServerCertificate :
    Entity<short>,
    IMatchDomainNames,
    IDisable
{
    /// <inheritdoc/>
    [Comment("匹配域名地址数组")]
    [Required]
    public required string MatchDomainNames { get; set; }

    /// <summary>
    /// HTTPS/TLS 证书（已加密）
    /// </summary>
    [Comment("HTTPS/TLS 证书（已加密）")]
    [Required]
    public required byte[] X509Certificate2 { get; set; }

    /// <summary>
    /// 证书的主题可分辨名称
    /// </summary>
    [Comment("证书的主题可分辨名称")]
    public string? Subject { get; set; }

    /// <summary>
    /// 大端十六进制字符串格式的证书的序列号
    /// </summary>
    [Comment("大端十六进制字符串格式的证书的序列号")]
    public string? SerialNumber { get; set; }

    /// <summary>
    /// 证书的生效日期
    /// </summary>
    [Comment("证书的生效日期")]
    public DateTimeOffset? NotBefore { get; set; }

    /// <summary>
    /// 证书的到期日期
    /// </summary>
    [Comment("证书的到期日期")]
    public DateTimeOffset NotAfter { get; set; }

    /// <summary>
    /// 证书的有效日期
    /// </summary>
    [Comment("证书的有效日期")]
    public string? EffectiveDateString { get; set; }

    /// <summary>
    /// 证书的到期日期
    /// </summary>
    [Comment("证书的到期日期")]
    public string? ExpirationDateString { get; set; }

    /// <summary>
    /// SHA256
    /// </summary>
    [StringLength(MaxLengths.SHA256)]
    [Comment("SHA256")]
    public string? SHA256 { get; set; }

    /// <summary>
    /// SHA1
    /// </summary>
    [StringLength(MaxLengths.SHA1)]
    [Comment("SHA1")]
    public string? SHA1 { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <summary>
    /// 禁用续订
    /// </summary>
    [Comment("禁用续订")]
    public bool DisableRenew { get; set; }

    /// <summary>
    /// 用于证书签发验证的密钥授权字符串
    /// </summary>
    [Comment("验证令牌")]
    public string? ValidateToken { get; set; }

    /// <summary>
    /// 用于证书签发验证的地址
    /// </summary>
    [Comment("验证 Url")]
    public string? ValidateUrl { get; set; }

    /// <summary>
    /// 是否自动同步 CDN
    /// </summary>
    [Comment("同步 CDN")]
    public bool SyncCDN { get; set; }

    /// <summary>
    /// 是否自动同步 Nginx
    /// </summary>
    [Comment("同步 Nginx")]
    public bool SyncNginx { get; set; }

    /// <summary>
    /// 同步 CDN 域名列表
    /// </summary>
    [Comment("同步 域名列表")]
    public string? SyncCDNDomainNamesList { get; set; }
}

partial class ServerCertificate
{
    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder b = new();
        b.AppendLine("Subject：");
        b.AppendLine(Subject);
        b.AppendLine("SerialNumber：");
        b.AppendLine(SerialNumber);
        b.AppendLine("PeriodValidity：");
        b.Append(EffectiveDateString);
        b.Append(" ~ ");
        b.Append(ExpirationDateString);
        b.AppendLine();
        b.AppendLine("SHA256：");
        b.AppendLine(SHA256);
        b.AppendLine("SHA1：");
        b.AppendLine(SHA1);

        return b.ToString();
    }

    public static ServerCertificate Convert(ServerCertificatePackable packable, Aes? aes, string? matchDomainNames = null)
    {
        using X509Certificate2? cert = packable;
        ArgumentNullException.ThrowIfNull(cert);

        if (string.IsNullOrWhiteSpace(matchDomainNames))
        {
            var subjectAlternativeNames = cert.GetSubjectAlternativeNames();
            if (subjectAlternativeNames != null && subjectAlternativeNames.Length != 0)
                matchDomainNames = string.Join(';', subjectAlternativeNames);
        }
        ArgumentNullException.ThrowIfNull(matchDomainNames);

        byte[] certBytes = packable;
        var certEncryptBytes = aes == null ? certBytes : aes.EncryptCbc(certBytes, aes.IV); // AES 加密

        ServerCertificate serverCertificate = new()
        {
            X509Certificate2 = certEncryptBytes,
            MatchDomainNames = matchDomainNames,
        };
        Map(cert, serverCertificate);
        return serverCertificate;
    }

    public static void EntityChange(ServerCertificatePackable packable, ServerCertificate serverCertificate, Aes? aes)
    {
        using X509Certificate2? cert = packable;
        ArgumentNullException.ThrowIfNull(cert);

        byte[] certBytes = packable;
        var certEncryptBytes = aes == null ? certBytes : aes.EncryptCbc(certBytes, aes.IV); // AES 加密

        serverCertificate.X509Certificate2 = certEncryptBytes;
        Map(cert, serverCertificate);
    }

    static void Map(X509Certificate2 cert, ServerCertificate serverCertificate)
    {
        serverCertificate.Subject = cert.Subject;
        serverCertificate.SerialNumber = cert.SerialNumber;
        serverCertificate.NotAfter = cert.NotAfter;
        serverCertificate.NotBefore = cert.NotBefore;
        serverCertificate.EffectiveDateString = cert.GetEffectiveDateString();
        serverCertificate.ExpirationDateString = cert.GetExpirationDateString();
        serverCertificate.SHA256 = cert.GetCertHashString(HashAlgorithmName.SHA256);
        serverCertificate.SHA1 = cert.GetCertHashString(HashAlgorithmName.SHA1);
    }

    public X509Certificate2? GetX509Certificate2(Aes? aes)
    {
        var certEncryptBytes = X509Certificate2;
        if (certEncryptBytes == null || certEncryptBytes.Length == 0)
        {
            return null;
        }
        var certBytes = aes == null ? certEncryptBytes : aes.DecryptCbc(certEncryptBytes, aes.IV); // AES 解密
        ServerCertificatePackable packable = certBytes;
        X509Certificate2? cert = packable;
        return cert;
    }
}