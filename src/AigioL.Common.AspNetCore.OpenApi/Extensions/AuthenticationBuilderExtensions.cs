using AigioL.Common.AspNetCore.OpenApi.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.Extensions.DependencyInjection._AA;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder Add3rdOpenApiBase<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this AuthenticationBuilder builder, Action<OpenApiAuthenticationSchemeOptions>? configureOptions = null)
        where THandler : OpenApiAuthenticationHandlerBase
    {
        builder.AddScheme<OpenApiAuthenticationSchemeOptions, THandler>("HMAC-SHA256", configureOptions);

        // https://github.com/dotnet/aspnetcore/blob/v10.0.9/src/Security/Authentication/Core/src/AuthenticationBuilder.cs#L34
        builder.Services.Configure<AuthenticationOptions>(o =>
        {
            o.AddScheme("HMAC-SHA384", scheme =>
            {
                scheme.HandlerType = typeof(THandler);
                //scheme.DisplayName = displayName;
            });
            o.AddScheme("HMAC-SHA512", scheme =>
            {
                scheme.HandlerType = typeof(THandler);
                //scheme.DisplayName = displayName;
            });
        });

        return builder;
    }

    public static AuthenticationBuilder Add3rdOpenApi<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDbContext>(
        this AuthenticationBuilder builder, Action<OpenApiAuthenticationSchemeOptions>? configureOptions = null)
        where TDbContext : DbContext
    {
        return builder.Add3rdOpenApiBase<OpenApiAuthenticationHandler<TDbContext>>(configureOptions);
    }

    public static TBuilder RequireAuthorizationWith3rdOpenApi<TBuilder>(
        this TBuilder builder,
        IAuthorizeData? authorizeData = null)
        where TBuilder : IEndpointConventionBuilder
    {
        IAuthorizeData[] authorizeData2 = authorizeData == null ?
            [HMAC_SHA256_AA, HMAC_SHA384_AA, HMAC_SHA512_AA] :
            [authorizeData, HMAC_SHA256_AA, HMAC_SHA384_AA, HMAC_SHA512_AA];
        return builder.RequireAuthorization(authorizeData2);
    }
}

#pragma warning disable IDE1006 // 命名样式
file static class _AA
#pragma warning restore IDE1006 // 命名样式
{
    internal static readonly AuthorizeAttribute HMAC_SHA256_AA = new() { AuthenticationSchemes = "HMAC-SHA256", };
    internal static readonly AuthorizeAttribute HMAC_SHA384_AA = new() { AuthenticationSchemes = "HMAC-SHA384", };
    internal static readonly AuthorizeAttribute HMAC_SHA512_AA = new() { AuthenticationSchemes = "HMAC-SHA512", };
}