using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;

public partial interface IIdentityJsonWebTokenValueProvider
{
    TokenValidationParameters GetTokenValidationParameters();

    Task<(JsonWebTokenValue? jwtData, string? jwtId)> GenerateTokenAsync(Guid userId,
        DevicePlatform2 platform,
        string? deviceId,
        IEnumerable<string>? roles,
        DateTimeOffset now = default,
        Action<List<Claim>>? aciton = null,
        CancellationToken cancellationToken = default);
}
