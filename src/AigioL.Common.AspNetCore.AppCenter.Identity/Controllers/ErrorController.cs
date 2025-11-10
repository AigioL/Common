using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// MVC 错误页面，固定路由 /Error
/// </summary>
public static partial class ErrorController
{
    public static void MapIdentityError(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "error")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("", async (HttpContext context) =>
        {
            ResponseCacheNone(context);
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature != null)
            {
                var r = await Get(context, exceptionHandlerPathFeature);
                return r;
            }
            else
            {
                return Results.NotFound();
            }
        });

#if DEBUG
        MapIdentityErrorTest(b);
#endif
    }

    public static async Task<IResult> Get(
        HttpContext context,
        IExceptionHandlerPathFeature exceptionHandlerPathFeature)
    {
        object? error;
        var path = exceptionHandlerPathFeature.Path;
        if (path != null && path.Contains("steam", StringComparison.InvariantCultureIgnoreCase) && (
            path.Contains("sign", StringComparison.InvariantCultureIgnoreCase) ||
            path.Contains("externallogin", StringComparison.InvariantCultureIgnoreCase)))
        {
            error = R.SteamLoginException;
        }
        else
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-6.0#exception-handler-lambda
            var traceId = context.GetTraceId();
            var b = new StringBuilder("An exception was thrown.");
            if (exceptionHandlerPathFeature.Error is FileNotFoundException)
            {
                b.AppendLine();
                b.Append("The file was not found.");
            }
            if (exceptionHandlerPathFeature.Path != "/")
            {
                b.AppendLine();
                b.Append($"path: {exceptionHandlerPathFeature.Path}");
            }
            b.AppendLine();
            b.Append($"traceId: {traceId}");
            error = b;
        }

#if DEBUG
        var repo = context.RequestServices.GetService<IKeyValuePairRepository>();
#else
        var repo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
#endif
        var layout = await repo.GetLayoutModelAsync(context.RequestAborted);
        layout.SetSubHeadTitle(R.ModelErrorTitle);
        LoginDetectionModel m = new()
        {
            Layout = layout,
            Error = error?.ToString(),
        };

        return m.ToResult(StatusCodes.Status500InternalServerError);
    }

    public static void ResponseCacheNone(HttpContext context)
    {
        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/response#nostore-and-locationnone

        context.Response.Headers.CacheControl = "no-store,no-cache";
        context.Response.Headers.Pragma = "no-cache";
    }

    const string ContentTypeHtml = "text/htmll; charset=utf-8";

    /// <summary>
    /// 标注终结点元数据，表示该终结点会返回 Identity UI 页面
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TBuilder WithIdentityUIView<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(IdentityUIViewMetadata.Instance)
        .ProducesProblem(StatusCodes.Status200OK, ContentTypeHtml)
        .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypeHtml);

    /// <summary>
    /// 处理 Identity UI 页面异常
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<bool> IdentityUIViewExceptionHandler(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var isUIView = endpoint.Metadata.OfType<IdentityUIViewMetadata>().Any();
            if (isUIView)
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature != null)
                {
                    var result = await Get(context, exceptionHandlerPathFeature);
                    await result.ExecuteAsync(context);
                    return true;
                }
            }
        }

        return false;
    }
}

/// <summary>
/// 表示该终结点会返回 Identity UI 页面的元数据类型
/// </summary>
file sealed class IdentityUIViewMetadata
{
    IdentityUIViewMetadata() { }

    /// <inheritdoc cref="IdentityUIViewMetadata"/>
    public static readonly IdentityUIViewMetadata Instance = new();
}

#if DEBUG
partial class ErrorController
{
    /// <summary>
    /// 仅用于测试
    /// </summary>
    /// <param name="b"></param>
    static void MapIdentityErrorTest(IEndpointRouteBuilder b)
    {
        b.MapGet("/identity/test/html/ex", async (HttpContext context) =>
        {
            var exceptionHandlerPathFeature = new ExceptionHandlerFeature
            {
                Path = context.Request.Path.Value!,
                Error = new ApplicationException("测试 AppException"),
            };
            var r = await Get(context, exceptionHandlerPathFeature);
            return r;
        }).AllowAnonymous();
        b.MapGet("/identity/test/html/ex/steam", async (HttpContext context) =>
        {
            var exceptionHandlerPathFeature = new ExceptionHandlerFeature
            {
                Path = "/signin-steam",
            };
            var r = await Get(context, exceptionHandlerPathFeature);
            return r;
        }).AllowAnonymous();
        b.MapGet("/identity/test/html/ex/empty", async (HttpContext context) =>
        {
            var exceptionHandlerPathFeature = new ExceptionHandlerFeature
            {
                Path = context.Request.Path.Value!,
            };
            var r = await Get(context, exceptionHandlerPathFeature);
            return r;
        }).AllowAnonymous();
        b.MapGet("/identity/test/html/ex/file", async (HttpContext context) =>
        {
            var exceptionHandlerPathFeature = new ExceptionHandlerFeature
            {
                Path = context.Request.Path.Value!,
                Error = new FileNotFoundException(),
            };
            var r = await Get(context, exceptionHandlerPathFeature);
            return r;
        }).AllowAnonymous();
        b.MapGet("/identity/test/html/ex/agg", async (HttpContext context) =>
        {
            var exceptionHandlerPathFeature = new ExceptionHandlerFeature
            {
                Path = context.Request.Path.Value!,
                Error = new AggregateException(
                    new ApplicationException(),
                    new DirectoryNotFoundException(),
                    new IOException(),
                    new Exception()),
            };
            var r = await Get(context, exceptionHandlerPathFeature);
            return r;
        }).AllowAnonymous();
    }
}
#endif