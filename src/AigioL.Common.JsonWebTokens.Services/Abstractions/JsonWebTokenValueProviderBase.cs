using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.JsonWebTokens.Models.Abstractions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AigioL.Common.JsonWebTokens.Services.Abstractions;

/// <summary>
/// JsonWebToken 值提供者默认实现基类
/// </summary>
public abstract class JsonWebTokenValueProviderBase<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions> : IJsonWebTokenValueProvider where TOptions : IJsonWebTokenOptions
{
    /// <summary>
    /// 随机数生成器
    /// </summary>
    static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    protected abstract TOptions GetOptions();

    protected abstract string? GetUserIdClaimType();

    public async ValueTask<JsonWebTokenValue?> GenerateTokenAsync(
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

        var userIdClaimType = GetUserIdClaimType() ?? ClaimTypes.Name;
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
            signingCredentials: options.SigningCredentials);
        var encodedJwt = handler.WriteToken(jwt);
        JsonWebTokenValue m = new()
        {
            AccessToken = encodedJwt,
            ExpiresIn = expires,
            RefreshToken = refresh_token,
        };
        return m;
    }

    /// <summary>
    /// 向 Claims 中添加角色信息
    /// </summary>
    static void AddRolesToClaims(List<Claim> claims, IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            var roleClaim = new Claim(ClaimTypes.Role, role);
            claims.Add(roleClaim);
        }
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    static string GenerateRefreshToken(string password)
    {
        // https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/PasswordHasher.cs#L141
        const int saltSize = 32;
        const int iterCount = 10000;
        const KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256;
        const int numBytesRequested = 32;
        var salt = new byte[saltSize];
        _rng.GetBytes(salt);
        var subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

        var outputBytes = new byte[13 + salt.Length + subkey.Length];
        outputBytes[0] = 0x01; // format marker
        WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
        WriteNetworkByteOrder(outputBytes, 5, iterCount);
        WriteNetworkByteOrder(outputBytes, 9, saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);

        var result = Base64Url.EncodeToString(outputBytes);
        return result;
    }

    /// <summary>
    /// 将无符号整型值以网络字节顺序写入字节数组中
    /// </summary>
    static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }

    protected abstract ValueTask AddOrUpdateRefreshTokenAsync(
        Guid userId,
        string refresh_token,
        DateTimeOffset refresh_token_expires,
        DateTimeOffset refresh_not_before,
        CancellationToken cancellationToken);
}