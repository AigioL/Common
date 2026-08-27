using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace AigioL.Common.Net.ReverseProxy.Internals.Logging;

/// <summary>
/// 请求日志中间件
/// </summary>
sealed partial class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
{
    readonly ILogger logger = logger;

    // https://github.com/dotnet/aspnetcore/blob/main/src/Hosting/Hosting/src/Internal/HostingApplication.cs#L18
    //const string DeprecatedDiagnosticsBeginRequestKey = "Microsoft.AspNetCore.Hosting.BeginRequest";

    /// <summary>
    /// 处理请求
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var feature = context.Features.Get<IRequestLoggingFeature>();
        if (feature == null)
        {
            feature = new RequestLoggingFeature();
            context.Features.Set(feature);
        }

        long startTimestamp = Stopwatch.GetTimestamp();
        long currentTimestamp;

        try
        {
            await next(context);
        }
        finally
        {
            currentTimestamp = Stopwatch.GetTimestamp();
        }

        if (!feature.Enable)
        {
            return;
        }

        var request = context.Request;
        var response = context.Response;
        var exception = context.GetForwarderErrorFeature()?.Exception;
        if (exception == null)
        {
            // 耗时由 Microsoft.AspNetCore.Hosting.Diagnostics 输出日志
            // url 与状态码由 Yarp.ReverseProxy.Forwarder.HttpForwarder 输出日志
            // 此处不再记录日志
            //LogInfo(logger, request.Method, request.Scheme, request.Host, request.Path, response.StatusCode, stopwatch.Elapsed.TotalMilliseconds);
        }
        else if (IsError(exception))
        {
            LogErr(logger, exception, request.Method, request.Scheme, request.Host, request.Path, response.StatusCode, Stopwatch.GetElapsedTime(startTimestamp, currentTimestamp).TotalMilliseconds);
        }
        else
        {
            LogWarn(logger, exception, request.Method, request.Scheme, request.Host, request.Path, response.StatusCode, Stopwatch.GetElapsedTime(startTimestamp, currentTimestamp).TotalMilliseconds);
        }
    }

    /// <summary>
    /// 是否为错误
    /// </summary>
    static bool IsError(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            return false;
        }
        if (HasInnerException<ConnectionAbortedException>(exception))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 是否有内部异常异常
    /// </summary>
    static bool HasInnerException<TInnerException>(Exception exception)
        where TInnerException : Exception
    {
        var inner = exception.InnerException;
        while (inner != null)
        {
            if (inner is TInnerException)
            {
                return true;
            }
            inner = inner.InnerException;
        }
        return false;
    }

    [LoggerMessage(
       Level = LogLevel.Error,
       Message = "{method} {scheme}://{host}{path} responded {statusCode} in {elapsed} ms")]
    private static partial void LogErr(
       ILogger logger, Exception ex, string method, string scheme, HostString host, PathString path, int statusCode, double elapsed);

    [LoggerMessage(
       Level = LogLevel.Warning,
       Message = "{method} {scheme}://{host}{path} responded {statusCode} in {elapsed} ms")]
    private static partial void LogWarn(
       ILogger logger, Exception ex, string method, string scheme, HostString host, PathString path, int statusCode, double elapsed);
}