namespace AigioL.Common.Storage.Models.Channels.TencentCloud;

public sealed record class TencentCloudOptions : IExplicitHasValue
{
    /// <summary>
    /// 地域, COS 地域的简称请参照 https://cloud.tencent.com/document/product/436/6224
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// 云 API 密钥 SecretId, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi
    /// </summary>
    public string? SecretId { get; set; }

    /// <summary>
    /// 云 API 密钥 SecretKey, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// 腾讯云账号 AppId
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 默认存储桶名称
    /// </summary>
    public string? DefaultBucket { get; set; }

    const long DefaultDurationSecond = 600;

    /// <summary>
    /// 每次请求签名有效时长，单位为秒
    /// </summary>
    public long? DurationSecond { get; set; }

    public long GetDurationSecond() => DurationSecond ?? DefaultDurationSecond;

    /// <summary>
    /// CDN 域名
    /// </summary>
    public string? CdnDomainName { get; set; }

    /// <summary>
    /// CDN 自定义密钥
    /// </summary>
    public string? CdnCustomKey { get; set; }

    bool IExplicitHasValue.ExplicitHasValue()
    {
        return !string.IsNullOrWhiteSpace(AppId) &&
            !string.IsNullOrWhiteSpace(Region) &&
            !string.IsNullOrWhiteSpace(SecretId) &&
            !string.IsNullOrWhiteSpace(SecretKey) &&
            !string.IsNullOrWhiteSpace(DefaultBucket);
    }
}
