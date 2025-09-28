using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;

public interface IJsonWebTokenUserManager
{
    /// <summary>
    /// 根据 RefreshToken 刷新 Token 与新的 JwtId
    /// </summary>
    Task<JsonWebTokenValue?> RefreshTokenAsync(
        DevicePlatform2 platform,
        string? deviceId,
        string refresh_token);
}
