using AigioL.Common.Storage.Models.Channels.TencentCloud;

namespace AigioL.Common.Storage.Models;

/// <summary>
/// 对象存储配置
/// </summary>
public sealed record class ObjectStorageOptions
{
    /// <summary>
    /// 腾讯云对象存储配置
    /// </summary>
    public TencentCloudOptions TencentCloud { get; set; } = new();

    /// <summary>
    /// 获取默认的提供商名称
    /// </summary>
    public static string? GetDefaultProviderName(ObjectStorageOptions? options)
    {
        if (options != null)
        {
            if (options.TencentCloud.HasValue())
                return nameof(TencentCloud);
        }
        return null;
    }
}
