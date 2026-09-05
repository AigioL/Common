namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public enum RefreshAccessTokenResultCode : byte
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 1,

    /// <summary>
    /// 刷新 Token 为空
    /// </summary>
    SteamRefreshTokenIsNull,

    /// <summary>
    /// 刷新 Token 值格式不正确
    /// </summary>
    SteamRefreshTokenIncorrectFormat,

    /// <summary>
    /// 调用 /jwt/ajaxrefresh 接口返回的响应为空
    /// </summary>
    JwtAjaxRefreshResponseIsNull,

    /// <summary>
    /// 根据上一个接口的响应生成下一个接口的请求模型为空，可能上一个接口缺少 Nonce 或 Auth 或 SteamId
    /// </summary>
    GetLoginSetTokenRequestIsNull,

    /// <summary>
    /// 响应的设置 Cookie 为空
    /// </summary>
    ResponseSetCookiesIsNull,
}