namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;

public interface IAppSettings
{
    /// <summary>
    /// 重定向到目标图片的 Url 地址，格式化字符串，例如 "https://cdn.example.com/images/{0}"
    /// </summary>
    string? ImageUrl { get; }

    /// <summary>
    /// 此业务官网 Url 地址
    /// </summary>
    string? OfficialWebsite { get; }

    #region COS 云存储配置

    /// <summary>
    /// 云存储 每次请求签名有效时长，单位为秒
    /// </summary>
    int COSDurationSecond { get; set; }

    /// <summary>
    /// 云存储 云 API 密钥 SecretId, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi
    /// </summary>
    string? COSSecretId { get; set; }

    /// <summary>
    /// 云储存 云 API 密钥 SecretKey, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi
    /// </summary>
    string? COSSecretKey { get; set; }

    /// <summary>
    /// 图片 云存储区域
    /// </summary>
    string ImageHandleCosRegion { get; set; }

    /// <summary>
    /// 图片 存储桶名称
    /// </summary>
    string ImageHandleCosBucket { get; set; }

    #endregion
}
