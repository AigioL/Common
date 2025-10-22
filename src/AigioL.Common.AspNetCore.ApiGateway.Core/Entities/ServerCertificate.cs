using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AigioL.Common.AspNetCore.ApiGateway.Entities;

/// <summary>
/// 服务器证书（HTTPS/TLS 证书）表实体类
/// </summary>
[Table("ServerCertificates")]
public sealed partial class ServerCertificate : Entity<short>, IDisable
{
    /// <inheritdoc/>
    [Comment("匹配域名地址数组")]
    [Required]
    [StringLength(MaxLengths.Url)]
    [Column(TypeName = "jsonb")] // https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Ccomplex-types%2Cjsondocument
    public required string MatchDomainNames { get; set; }

    /// <summary>
    /// HTTPS/TLS 证书（已加密）
    /// </summary>
    [Comment("HTTPS/TLS 证书（已加密）")]
    [Required]
    public required byte[] X509Certificate2 { get; set; }

    /// <inheritdoc cref="X509Certificate.Subject"/>
    [Comment("证书的主题可分辨名称")]
    [StringLength(256)]
    public string? Subject { get; set; }

    /// <inheritdoc cref="X509Certificate2.SerialNumber"/>
    [Comment("大端十六进制字符串格式的证书的序列号")]
    [StringLength(256)]
    public string? SerialNumber { get; set; }

    /// <inheritdoc cref="X509Certificate2.NotBefore"/>
    [Comment("证书的生效日期")]
    public DateTimeOffset? NotBefore { get; set; }

    /// <inheritdoc cref="X509Certificate2.NotAfter"/>
    [Comment("证书的到期日期")]
    public DateTimeOffset NotAfter { get; set; }

    /// <inheritdoc cref="X509Certificate.GetEffectiveDateString"/>
    [Comment("证书的有效日期")]
    [StringLength(256)]
    public string? EffectiveDateString { get; set; }

    /// <inheritdoc cref="X509Certificate.GetExpirationDateString"/>
    [Comment("证书的到期日期")]
    [StringLength(256)]
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
    [StringLength(MaxLengths.Url)]
    public string? ValidateToken { get; set; }

    /// <summary>
    /// 用于证书签发验证的地址
    /// </summary>
    [Comment("验证 Url")]
    [StringLength(MaxLengths.Url)]
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
    [Comment("同步 CDN 域名列表")]
    [StringLength(MaxLengths.Url)]
    [Column(TypeName = "jsonb")] // https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Ccomplex-types%2Cjsondocument
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

    //public static ServerCertificate Convert(ServerCertificatePackable packable, string? matchDomainNames = null)
    //{
    //    using X509Certificate2? cert = packable;
    //    cert.ThrowIsNull();

    //    if (string.IsNullOrWhiteSpace(matchDomainNames))
    //    {
    //        var subjectAlternativeNames = cert.GetSubjectAlternativeNames();
    //        if (subjectAlternativeNames != null && subjectAlternativeNames.Count != 0)
    //            matchDomainNames = string.Join(';', subjectAlternativeNames);
    //    }

    //    byte[] certBytes = packable;
    //    var certEncryptBytes = IAppSettings.Instance.Aes.Encrypt(certBytes); // AES 加密

    //    ServerCertificate serverCertificate = new()
    //    {
    //        X509Certificate2 = certEncryptBytes,
    //        MatchDomainNames = matchDomainNames,
    //    };
    //    Map(cert, serverCertificate);
    //    return serverCertificate;
    //}

    //public static void EntityChange(ServerCertificatePackable packable, ServerCertificate serverCertificate)
    //{
    //    using X509Certificate2? cert = packable;
    //    cert.ThrowIsNull();

    //    byte[] certBytes = packable;
    //    var certEncryptBytes = IAppSettings.Instance.Aes.Encrypt(certBytes); // AES 加密

    //    serverCertificate.X509Certificate2 = certEncryptBytes;
    //    Map(cert, serverCertificate);
    //}

    //private static void Map(X509Certificate2 cert, ServerCertificate serverCertificate)
    //{
    //    serverCertificate.Subject = cert.Subject;
    //    serverCertificate.SerialNumber = cert.SerialNumber;
    //    serverCertificate.NotAfter = cert.NotAfter;
    //    serverCertificate.NotBefore = cert.NotBefore;
    //    serverCertificate.EffectiveDateString = cert.GetEffectiveDateString();
    //    serverCertificate.ExpirationDateString = cert.GetExpirationDateString();
    //    serverCertificate.SHA256 = cert.GetCertHashStringCompat(HashAlgorithmName.SHA256);
    //    serverCertificate.SHA1 = cert.GetCertHashStringCompat(HashAlgorithmName.SHA1);
    //}

    //public static implicit operator X509Certificate2?(ServerCertificate value)
    //{
    //    var certEncryptBytes = value.X509Certificate2;
    //    var certBytes = IAppSettings.Instance.Aes.Decrypt(certEncryptBytes.ThrowIsNull()); // AES 解密
    //    ServerCertificatePackable packable = certBytes;
    //    X509Certificate2? cert = packable;
    //    return cert;
    //}
}