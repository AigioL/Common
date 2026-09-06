namespace AigioL.Common.Stm.Constants;

/// <summary>
/// Steam Cookie 的常量类
/// </summary>
public static partial class SteamCookieConstants
{
    /// <summary>
    /// 访问 Token 与 刷新 Token 值的分隔符
    /// </summary>
    public const string Separator = "%7C%7C";

    /// <summary>
    /// Steam 访问 Token Cookie 键
    /// </summary>
    public const string CookieKeySteamAccessToken = "steamLoginSecure";

    /// <summary>
    /// Steam 刷新 Token Cookie 键
    /// </summary>
    public const string CookieKeySteamRefreshToken = "steamRefresh_steam";

    public const string CookieKeySteamWebTradeEligibility = "webTradeEligibility";

    /// <summary>
    /// Steam 网页上的会话 Id Cookie 键
    /// </summary>
    public const string CookieKeySteamWebPageSessionId = "sessionid";

    /// <summary>
    /// Jwt 受众 - Steam 社区
    /// </summary>
    public const string JwtAudienceSteamCommunity = "web:community";
}
