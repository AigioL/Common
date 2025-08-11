namespace AigioL.Common.JsonWebTokens.Entities.Abstractions;

/// <summary>
/// 刷新 JsonWebToken 相关数据
/// </summary>
public interface IRefreshJsonWebTokenUser
{
    /// <summary>
    /// 刷新 Token 值
    /// </summary>
    string? RefreshToken { get; set; }

    /// <summary>
    /// 刷新 Token 值有效期
    /// </summary>
    DateTimeOffset RefreshExpiration { get; set; }

    /// <summary>
    /// 禁止在此时间之前刷新 Token
    /// </summary>
    DateTimeOffset NotBefore { get; set; }
}
