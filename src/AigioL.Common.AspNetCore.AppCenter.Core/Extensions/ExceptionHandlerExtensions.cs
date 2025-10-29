using AigioL.Common.Models;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class ExceptionHandlerExtensions
{
    /// <summary>
    /// 使用 <see cref="ApiRsp"/> 格式的异常处理与状态码响应
    /// <para>https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/error-handling</para>
    /// </summary>
    /// <param name="app"></param>
    /// <param name="handlerException">可重写异常处理，返回 <see langword="true"/> 中断 <see cref="ApiRsp"/> 格式的行为</param>
    /// <returns></returns>
    public static IApplicationBuilder UseApiRspExceptionHandler(
        this IApplicationBuilder app,
        Func<HttpContext, Task<bool>>? handlerException = null)
        => app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                if (handlerException != null)
                {
                    var isReturn = await handlerException(context);
                    if (isReturn)
                    {
                        return;
                    }
                }

                var path = context.GetExceptionHandlerPath(out var error);
                ApiRsp apiRsp = new();
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
                await apiRsp.WriteAsync(
                    context.Response,
                    cancellationToken: context.RequestAborted);
            });
        }).UseStatusCodePages(statusCodePagesApp =>
        {
            statusCodePagesApp.Run(async context =>
            {
                var traceId = context.GetTraceId();
                ApiRsp apiRsp = new()
                {
                    Code = unchecked((uint)context.Response.StatusCode),
                    Url = context.Request.Path,
                    TraceId = traceId,
                };

                context.Response.StatusCode = StatusCodes.Status200OK;
                await apiRsp.WriteAsync(
                    context.Response,
                    cancellationToken: context.RequestAborted);
            });
        });
}
