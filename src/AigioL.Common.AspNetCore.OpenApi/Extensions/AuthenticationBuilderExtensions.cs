using AigioL.Common.AspNetCore.OpenApi.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

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
}
