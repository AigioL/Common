using AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

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

    public static void MapACMinimalApis(this IEndpointRouteBuilder b)
    {
        b.MapPostInfo();

        b.MapGetIpV6();
        b.MapGetIpVal();
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
}
