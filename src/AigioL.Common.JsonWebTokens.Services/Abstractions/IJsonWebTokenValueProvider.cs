using AigioL.Common.JsonWebTokens.Models;
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
    ValueTask<JsonWebTokenValue> GenerateTokenAsync(
        Guid userId,
        IEnumerable<string>? roles,
        Action<List<Claim>>? aciton,
        bool generateRefreshToken = true,
        CancellationToken cancellationToken = default);
}
