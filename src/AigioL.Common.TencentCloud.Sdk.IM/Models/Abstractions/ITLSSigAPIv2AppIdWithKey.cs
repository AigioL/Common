namespace AigioL.Common.TencentCloud.Sdk.IM.Models.Abstractions;

/// <summary>
/// https://cloud.tencent.com/document/product/269/32688
/// </summary>
public interface ITLSSigAPIv2AppIdWithKey
{
    /// <summary>
    /// 应用 SDKAppID，可在即时通信 IM 控制台的应用卡片中获取
    /// </summary>
    string SdkAppId { get; }

    /// <summary>
    /// 密钥信息，可在即时通信 IM 控制台 的应用详情页面中获取
    /// </summary>
    string Key { get; }
}
