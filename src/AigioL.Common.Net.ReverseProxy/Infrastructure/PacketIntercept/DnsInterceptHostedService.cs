using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;
using static AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept.LoggerMessages;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

sealed class DnsInterceptHostedService : BackgroundService
{
    readonly ILogger logger;
    readonly IDnsInterceptor dnsInterceptor;
    readonly IReverseProxyWebAppLifecycle app;

    public DnsInterceptHostedService(
        ILogger<DnsInterceptHostedService> logger,
        IDnsInterceptor dnsInterceptor,
        IReverseProxyWebAppLifecycle app)
    {
        this.logger = logger;
        this.dnsInterceptor = dnsInterceptor;
        this.app = app;
    }

    /// <inheritdoc/>
    protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dnsInterceptor.InterceptAsync(cancellationToken);
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
