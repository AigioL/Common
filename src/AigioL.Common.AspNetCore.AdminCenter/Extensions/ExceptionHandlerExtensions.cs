using AigioL.Common.AspNetCore.AdminCenter.Models;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class ExceptionHandlerExtensions
{
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
}
