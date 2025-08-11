using System.Security.Claims;

namespace AigioL.Common.JsonWebTokens.Services.Abstractions;

/// <summary>
/// JsonWebToken 值提供者接口
/// </summary>
public interface IJsonWebTokenValueProvider
{
    /// <summary>
    /// 生成 JsonWebToken
    /// </summary>
    /// <returns></returns>
    ValueTask<(DateTimeOffset expiresIn, string accessToken, string? refreshToken)?> GenerateTokenAsync(
        Guid userId,
        IEnumerable<string>? roles,
        Action<List<Claim>>? aciton,
        bool generateRefreshToken = true,
        CancellationToken cancellationToken = default);
}
