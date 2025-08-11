using AigioL.Common.JsonWebTokens.Models.Abstractions;
using AigioL.Common.JsonWebTokens.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.JsonWebTokens.Services;

/// <summary>
/// JsonWebToken 值提供者默认实现
/// </summary>
public class JsonWebTokenValueProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(
    IOptions<TOptions> options,
    IOptions<IdentityOptions> identityOptions) :
    JsonWebTokenValueProviderBase<TOptions> where TOptions : class, IJsonWebTokenOptions
{
    protected sealed override TOptions GetOptions() => options.Value;

    protected sealed override string? GetUserIdClaimType() => identityOptions.Value.ClaimsIdentity?.UserIdClaimType;

    protected override ValueTask AddOrUpdateRefreshTokenAsync(Guid userId, string refresh_token, DateTimeOffset refresh_token_expires, DateTimeOffset refresh_not_before, CancellationToken cancellationToken)
    {
        // 刷新 Token 的存储和管理需要由具体的应用程序实现，例如使用数据库单独表实体实现存储重写此函数
        throw new NotSupportedException();
    }
}
