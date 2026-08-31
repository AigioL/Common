using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;
using static AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept.LoggerMessages;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

sealed class TcpInterceptHostedService : BackgroundService
{
    readonly ILogger logger;
    readonly IEnumerable<ITcpInterceptor> tcpInterceptors;
    readonly IReverseProxyWebAppLifecycle app;

    public TcpInterceptHostedService(
        ILogger<TcpInterceptHostedService> logger,
        IEnumerable<ITcpInterceptor> tcpInterceptors,
        IReverseProxyWebAppLifecycle app)
    {
        this.logger = logger;
        this.tcpInterceptors = tcpInterceptors;
        this.app = app;
    }

    /// <inheritdoc/>
    protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tasks = tcpInterceptors.Select(item => item.InterceptAsync(cancellationToken));
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_OPERATION_ABORTED)
        {
        }
        catch (Exception ex)
        {
            LogErrorBackgroundServiceExecuteAsync(logger, ex);
            app.Stop();
        }
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "后台服务发生错误")]
    internal static partial void LogErrorBackgroundServiceExecuteAsync(ILogger logger, Exception? exception);
}