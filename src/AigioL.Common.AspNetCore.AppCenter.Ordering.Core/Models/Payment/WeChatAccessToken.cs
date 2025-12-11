namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 微信授权交互访问令牌
/// </summary>
public sealed partial class WeChatAccessToken
{
    /// <summary>
    /// 类型 1小程序，2公众号
    /// </summary>
    public int AppType { get; set; }

    /// <summary>
    /// 微信 appId
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信 accessToken
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 截止时间
    /// </summary>
    public long ExpireTimestamp { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long UpdateTimestamp { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreateTimestamp { get; set; }
}
