using AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;
using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;
using AigioL.Common.AspNetCore.AdminCenter.Repositories;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter;

/// <summary>
/// 管理后台（Admin Center）的最小 API
/// </summary>
public static partial class BMMinimalApis
{
    public const string BearerScheme = "Bearer";
    public const string BearerSchemeLower = "bearer";

    /// <summary>
    /// 注册管理后台的最小 API 路由
    /// </summary>
    public static void MapBMMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole>(this IEndpointRouteBuilder b)
        where TUser : BMUser, new()
        where TRole : BMRole, new()
    {
        b.MapPostInfo();
        b.MapBMLogin<TUser>();
        b.MapBMMenus();
        b.MapBMRoles<TRole>();
        b.MapBMUser<TUser>();
        b.MapBMUsers<TUser>();
    }

    /// <summary>
    /// 使用 <see cref="BMApiRsp"/> 格式的异常处理与状态码响应
    /// <para>https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/error-handling</para>
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseBMApiRspExceptionHandler(this IApplicationBuilder app) => app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var path = context.GetExceptionHandlerPath(out var error);
            BMApiRsp apiRsp = new();
            if (error != null)
            {
                apiRsp.SetException(error);
            }
            else
            {
                apiRsp.Code = StatusCodes.Status500InternalServerError;
            }
            apiRsp.Url = path;
            var traceId = context.GetTraceId();
            apiRsp.TraceId = traceId;

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(apiRsp,
                BMMinimalApisJsonSerializerContext.Default.BMApiRsp,
                cancellationToken: context.RequestAborted);
        });
    }).UseStatusCodePages(statusCodePagesApp =>
    {
        statusCodePagesApp.Run(async context =>
        {
            var traceId = context.GetTraceId();
            BMApiRsp apiRsp = new()
            {
                Code = unchecked((uint)context.Response.StatusCode),
                Url = context.Request.Path,
                TraceId = traceId,
            };

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(apiRsp,
                BMMinimalApisJsonSerializerContext.Default.BMApiRsp,
                cancellationToken: context.RequestAborted);
        });
    });

    /// <summary>
    /// 给响应设置 HTTP 上下文信息，通常在返回之前末尾调用，以设置跟踪 Id 与请求地址
    /// </summary>
    /// <typeparam name="TApiRspAC"></typeparam>
    /// <param name="apiRsp"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TApiRspAC SetHttpContext<TApiRspAC>(this TApiRspAC apiRsp, HttpContext context) where TApiRspAC : BMApiRsp
    {
        if (string.IsNullOrWhiteSpace(apiRsp.Url))
        {
            apiRsp.Url = context.Request.Path;
        }
        if (string.IsNullOrWhiteSpace(apiRsp.TraceId))
        {
            apiRsp.TraceId = context.GetTraceId();
        }
        return apiRsp;
    }

    /// <summary>
    /// 配置权限过滤器
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="controllerName"></param>
    /// <param name="buttonType"></param>
    /// <returns></returns>
    public static TBuilder PermissionFilter<TBuilder>(this TBuilder builder, string controllerName, BMButtonType buttonType = default)
        where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(new PermissionAuthorizationRequirement(controllerName, buttonType));

    /// <summary>
    /// 添加管理后台的仓储层服务接口
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TUserRole"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddACRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole>(this IServiceCollection services)
        where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>
        where TUser : BMUser
        where TRole : BMRole
        where TUserRole : BMUserRole
    {
        services.AddScoped<IUserManagerExtensions, UserManagerExtensions<TDbContext, TUser, TRole, TUserRole>>();
        services.TryAddScoped<IBMUserRepository, BMUserRepository<TDbContext, TUser, TRole, TUserRole>>();
        services.TryAddScoped<IBMRoleRepository, BMRoleRepository<TDbContext, TUser, TRole, TUserRole>>();
        services.TryAddScoped<IBMMenuRepository, BMMenuRepository<TDbContext, TUser, TRole, TUserRole>>();
        return services;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取管理后台用户 Id
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public static Guid GetACUserId(this HttpContext ctx)
    {
        var userManager = ctx.RequestServices.GetRequiredService<IUserManagerExtensions>();
        var userId = userManager.GetUserId(ctx);
        return userId;
    }

    /// <summary>
    /// 添加 Bearer 安全方案（JWT）转换器到 OpenAPI 文档转换器
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static OpenApiOptions AddBMBearerSecuritySchemeTransformer(this OpenApiOptions options) => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
}

file interface IUserManagerExtensions
{
    Guid GetUserId(HttpContext ctx);
}

file sealed class UserManagerExtensions<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    IUserManagerExtensions
    where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>
    where TUser : BMUser
    where TRole : BMRole
    where TUserRole : BMUserRole
{
    readonly TDbContext db;

#pragma warning disable IDE0290 // 使用主构造函数
    public UserManagerExtensions(TDbContext db)
#pragma warning restore IDE0290 // 使用主构造函数
    {
        this.db = db;
    }

    public Guid GetUserId(HttpContext ctx)
    {
        var userId = db.GetUserId(ctx);
        if (!userId.HasValue)
        {
#pragma warning disable CA2208 // 正确实例化参数异常
            throw new ArgumentNullException(nameof(userId));
#pragma warning restore CA2208 // 正确实例化参数异常
        }
        return userId.Value;
    }
}

/// <summary>
/// https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/openapi/customize-openapi#use-document-transformers
/// </summary>
/// <param name="authenticationSchemeProvider"></param>
file sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == BMMinimalApis.BearerScheme))
        {
#if NET10_0_OR_GREATER
            var requirements = new Dictionary<string, IOpenApiSecurityScheme>
#else
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
#endif
            {
                [BMMinimalApis.BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = BMMinimalApis.BearerSchemeLower, // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }
    }
}