using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.JsonWebTokens.Models.Abstractions;
using AigioL.Common.JsonWebTokens.Services.Abstractions;
using AigioL.Common.Primitives.Models;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services;

sealed partial class IdentityJsonWebTokenValueProvider<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions,
    TDbContext>(
    IOptions<TOptions> options,
    TDbContext db,
    IConnectionMultiplexer connection) :
    JsonWebTokenValueProviderBase<TOptions>, IIdentityJsonWebTokenValueProvider
    where TOptions : class, IJsonWebTokenOptions
    where TDbContext : DbContext, IIdentityDbContext
{
    TokenValidationParameters? tokenValidationParameters;

    protected override TOptions GetOptions() => options.Value;

    public TokenValidationParameters GetTokenValidationParameters()
    {
        if (tokenValidationParameters == null)
        {
            var signingKey = IJsonWebTokenOptions.GetSymmetricSecurityKey(options.Value);
            tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true, // 签名的密钥必须校验
                ValidateLifetime = true, // 验证凭证的时间是否过期
                ClockSkew = TimeSpan.Zero, // 时间不能有偏差
                IssuerSigningKey = signingKey,
                ValidIssuer = options.Value.Issuer,
                ValidateIssuer = true,
                ValidAudience = options.Value.Audience,
                ValidateAudience = true,
            };
        }
        return tokenValidationParameters;
    }

    public async Task<(JsonWebTokenValue? jwtData, string? jwtId)> GenerateTokenAsync(Guid userId,
        DevicePlatform2 platform,
        string? deviceId,
        IEnumerable<string>? roles,
        DateTimeOffset now_ = default,
        Action<List<Claim>>? aciton = null,
        CancellationToken cancellationToken = default)
    {
        var options = GetOptions();

        var now = now_ != default ? now_.ToUniversalTime() : DateTime.UtcNow;

        JwtSecurityTokenHandler handler = new();

        // Token 过期时间
        var expires = now.Add(options.AccessExpiration);

        // https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/PasswordHasher.cs#L96
        var refresh_token = GenerateRefreshToken(userId.ToString("N"));

        // 刷新 Token 过期时间
        var refresh_token_expires = expires.Add(options.RefreshExpiration);
        // 刷新 Token 必须在过期前 7 天后才能使用
        var refresh_not_before = expires.AddDays(-7);

        var jwtUserId = await AddOrUpdateTokenReturnJwtIdAsync(
             userId, platform, deviceId, refresh_token, refresh_token_expires, refresh_not_before,
             cancellationToken);

        if (jwtUserId == default) return default;

        var idString = ShortGuid.Encode(jwtUserId);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, idString),
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
            ExpiresIn = expires,
            AccessToken = encodedJwt,
            RefreshToken = refresh_token,
        };
        return (m, idString);
    }

    async Task<Guid> AddOrUpdateTokenReturnJwtIdAsync(
        Guid userId,
        DevicePlatform2 platform,
        string? deviceId,
        string refresh_token,
        DateTimeOffset refresh_token_expires,
        DateTimeOffset refresh_not_before,
        CancellationToken cancellationToken)
    {
        try
        {
            var redisdb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
            // 旧版无设备 Id，需要挤下线，与旧版服务端行为一致
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = null;
                var oldItem = await db.UserJsonWebTokens.Include(x => x.UserDevice)
                    .FirstOrDefaultAsync(x => x.UserDevice != null &&
                        x.UserDevice.Platform == platform &&
                        x.UserDevice.UserId == userId, cancellationToken);
                if (oldItem != null)
                {
                    // 删除映射，使之前的 Token 失效
                    db.UserJsonWebTokens.Remove(oldItem);
                    await db.SaveChangesAsync(cancellationToken);
                    await redisdb.HashDeleteAsync(CacheKeys.IdentityUserDeviceIsTrustWithUserIdMapHashKey, ShortGuid.Encode(oldItem.Id));
                }
            }

            var userDevice = await db.UserDevices.AsNoTrackingWithIdentityResolution()
                .Where(x => x.UserId == userId && x.Platform == platform && x.DeviceId == deviceId)
                .Select(x => new UserDeviceIsTrustWithId(x.Id, x.IsTrust))
                .FirstOrDefaultAsync(cancellationToken);

            UserJsonWebToken newItem;

            if (userDevice == null)
            {
                newItem = new()
                {
                    UserDevice = new()
                    {
                        UserId = userId,
                        Platform = platform,
                        DeviceId = deviceId,
                        LastLoginTime = DateTimeOffset.Now
                    },
                };
            }
            else
            {
                if (!userDevice.IsTrust)
                {
                    return default;
                }

                newItem = new()
                {
                    UserDeviceId = userDevice.Id,
                };
            }

            newItem.Refresh = new()
            {
                RefreshToken = refresh_token,
                RefreshExpiration = refresh_token_expires,
                NotBefore = refresh_not_before,
            };

            await db.UserJsonWebTokens.AddAsync(newItem, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await db.UserDevices.Where(x =>
                     x.UserId == userId &&
                     x.Platform == platform &&
                     x.DeviceId == deviceId)
                   .ExecuteUpdateAsync(x =>
                   x.SetProperty(u => u.LastLoginTime, u => DateTimeOffset.Now), cancellationToken);
            var hashField = ShortGuid.Encode(newItem.Id);
            var hashValue = MemoryPackSerializer.Serialize(new UserDeviceIsTrustWithUserId(userId, true));
            await redisdb.HashSetAsync(CacheKeys.IdentityUserDeviceIsTrustWithUserIdMapHashKey, hashField, hashValue);
            return newItem.Id;
        }
        catch (OperationCanceledException)
        {
            return default;
        }
    }
}
