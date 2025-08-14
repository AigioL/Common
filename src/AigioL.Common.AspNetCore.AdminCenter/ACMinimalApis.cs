using AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;
using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;
using AigioL.Common.AspNetCore.AdminCenter.Repositories;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter;

/// <summary>
/// 管理后台（Admin Center）的最小 API
/// </summary>
public static partial class ACMinimalApis
{
    /// <summary>
    /// 控制器名称最大长度限制
    /// </summary>
    public const int ControllerNameMaxLength = 128;

    /// <summary>
    /// 注册管理后台的最小 API 路由
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <param name="b"></param>
    public static void MapACMinimalApis<TUser>(this IEndpointRouteBuilder b) where TUser : ACUser
    {
        b.MapPostInfo();
        b.MapBMLogin<TUser>();
        b.MapBMMenus();
        b.MapBMRoles();
        b.MapBMUser();
        b.MapBMUsers();
    }

    static string GetPath(HttpContext context, out Exception? error)
    {
        error = null;
        string path;
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            path = exceptionHandlerPathFeature.Path;
            error = exceptionHandlerPathFeature.Error;
        }
        else
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                path = exceptionHandlerFeature.Path;
                error = exceptionHandlerFeature.Error;
            }
            else
            {
                path = context.Request.Path;
            }
        }
        return path;
    }

    static string GetTraceId(HttpContext context)
    {
        // https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs#L58
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        return traceId;
    }

    /// <summary>
    /// 使用 <see cref="ApiRspAC"/> 格式的异常处理与状态码响应
    /// <para>https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/error-handling</para>
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseApiRspACExceptionHandler(this IApplicationBuilder app) => app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var path = GetPath(context, out var error);
            ApiRspAC apiRsp = new();
            if (error != null)
            {
                apiRsp.SetException(error);
            }
            else
            {
                apiRsp.Code = StatusCodes.Status500InternalServerError;
            }
            apiRsp.Url = path;
            var traceId = GetTraceId(context);
            apiRsp.TraceId = traceId;

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(apiRsp,
                ACMinimalApisJsonSerializerContext.Default.ApiRspAC,
                cancellationToken: context.RequestAborted);
        });
    }).UseStatusCodePages(statusCodePagesApp =>
    {
        statusCodePagesApp.Run(async context =>
        {
            var traceId = GetTraceId(context);
            ApiRspAC apiRsp = new()
            {
                Code = unchecked((uint)context.Response.StatusCode),
                Url = context.Request.Path,
                TraceId = traceId,
            };

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(apiRsp,
                ACMinimalApisJsonSerializerContext.Default.ApiRspAC,
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
    public static TApiRspAC SetHttpContext<TApiRspAC>(this TApiRspAC apiRsp, HttpContext context) where TApiRspAC : ApiRspAC
    {
        if (string.IsNullOrWhiteSpace(apiRsp.Url))
        {
            apiRsp.Url = context.Request.Path;
        }
        if (string.IsNullOrWhiteSpace(apiRsp.TraceId))
        {
            apiRsp.TraceId = GetTraceId(context);
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
    public static TBuilder PermissionFilter<TBuilder>(this TBuilder builder, string controllerName, ACButtonType buttonType = default)
        where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(new PermissionAuthorizationRequirement(controllerName, buttonType));

    /// <summary>
    /// 添加管理后台的仓储层服务接口
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TACUser"></typeparam>
    /// <typeparam name="TACRole"></typeparam>
    /// <typeparam name="TACUserRole"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddACRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACRole,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUserRole>(this IServiceCollection services)
        where TDbContext : ACDbContextBase<TACUser, TACRole, TACUserRole>
        where TACUser : ACUser
        where TACRole : ACRole
        where TACUserRole : ACUserRole
    {
        services.AddScoped<IUserManagerExtensions, UserManagerExtensions<TDbContext, TACUser, TACRole, TACUserRole>>();
        services.TryAddScoped<IACUserRepository, ACUserRepository<TDbContext, TACUser, TACRole, TACUserRole>>();
        services.TryAddScoped<IACRoleRepository, ACRoleRepository<TDbContext, TACUser, TACRole, TACUserRole>>();
        services.TryAddScoped<IACMenuRepository, ACMenuRepository<TDbContext, TACUser, TACRole, TACUserRole>>();
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
}

file interface IUserManagerExtensions
{
    Guid GetUserId(HttpContext ctx);
}

file sealed class UserManagerExtensions<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUserRole> :
    IUserManagerExtensions
    where TDbContext : ACDbContextBase<TACUser, TACRole, TACUserRole>
    where TACUser : ACUser
    where TACRole : ACRole
    where TACUserRole : ACUserRole
{
    readonly TDbContext db;

    public UserManagerExtensions(TDbContext db)
    {
        this.db = db;
    }

    public Guid GetUserId(HttpContext ctx)
    {
        var userId = db.GetUserId(ctx);
        ArgumentNullException.ThrowIfNull(userId);
        return userId.Value;
    }
}