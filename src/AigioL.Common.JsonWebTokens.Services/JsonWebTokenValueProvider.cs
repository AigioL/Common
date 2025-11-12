using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.JsonWebTokens.Models.Abstractions;
using AigioL.Common.JsonWebTokens.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AigioL.Common.JsonWebTokens.Services;

/// <summary>
/// JsonWebToken 值提供者默认实现
/// </summary>
public class JsonWebTokenValueProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(
    IOptions<TOptions> options,
    IOptions<IdentityOptions> identityOptions) :
    JsonWebTokenValueProviderBase<TOptions>, IJsonWebTokenValueProvider
    where TOptions : class, IJsonWebTokenOptions
{
    protected sealed override TOptions GetOptions() => options.Value;

    public async ValueTask<JsonWebTokenValue> GenerateTokenAsync(
        Guid userId,
        IEnumerable<string>? roles,
        Action<List<Claim>>? aciton,
        bool generateRefreshToken = true,
        CancellationToken cancellationToken = default)
    {
        var options = GetOptions();

        var now = DateTimeOffset.UtcNow;

        JwtSecurityTokenHandler handler = new();

        // Token 过期时间
        var expires = now.Add(options.AccessExpiration);

        var idString = userId.ToString();

        string? refresh_token = null;

        if (generateRefreshToken)
        {
            // https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/PasswordHasher.cs#L96
            refresh_token = GenerateRefreshToken(idString);

            // 刷新 Token 过期时间
            var refresh_token_expires = expires.Add(options.RefreshExpiration);
            // 刷新 Token 必须在过期前 7 天后才能使用
            var refresh_not_before = expires.AddDays(-7);

            await AddOrUpdateRefreshTokenAsync(
                userId, refresh_token, refresh_token_expires, refresh_not_before,
                cancellationToken);
        }

        var userIdClaimType = identityOptions.Value.ClaimsIdentity?.UserIdClaimType ?? ClaimTypes.Name;
        var claims = new List<Claim>
        {
            new(userIdClaimType, idString),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeMilliseconds().ToString(), ClaimValueTypes.Integer64),
        };

        if (roles != null)
        {
            AddRolesToClaims(claims, roles);
        }

        aciton?.Invoke(claims);

        var jwt = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.DateTime,
            expires: expires.DateTime,
            signingCredentials: GetSigningCredentials()
            );
        var encodedJwt = handler.WriteToken(jwt);
        JsonWebTokenValue m = new()
        {
            AccessToken = encodedJwt,
            ExpiresIn = expires,
            RefreshToken = refresh_token,
        };
        return m;
    }

    ValueTask AddOrUpdateRefreshTokenAsync(Guid userId, string refresh_token, DateTimeOffset refresh_token_expires, DateTimeOffset refresh_not_before, CancellationToken cancellationToken)
    {
        // 刷新 Token 的存储和管理需要由具体的应用程序实现，例如使用数据库单独表实体实现存储重写此函数
        throw new NotImplementedException("TODO: RefreshToken");
    }
}
