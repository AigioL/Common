using AigioL.Common.Net.ReverseProxy.Infrastructure.FlowAnalyzer;
using AigioL.Common.Net.ReverseProxy.Infrastructure.Tls;
using AigioL.Common.Net.ReverseProxy.Services.Abstractions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Hosting;

/// <summary>
/// <see cref="ListenOptions"/> 的扩展方法
/// </summary>
public static partial class ListenOptionsExtensions
{
    /// <summary>
    /// 使用流量分析中间件
    /// </summary>
    public static ListenOptions UseFlowAnalyze(this ListenOptions listen)
    {
        var flowAnalyzer = listen.ApplicationServices.GetRequiredService<IFlowAnalyzer>();
        listen.Use(next => async context =>
        {
            var oldTransport = context.Transport;
            try
            {
                await using var adapter = new DuplexPipeStreamAdapter<FlowAnalyzeStream>(
                    context.Transport,
                    stream => new(stream, flowAnalyzer));
                context.Transport = adapter;
                await next(context);
            }
            finally
            {
                context.Transport = oldTransport;
            }
        });
        return listen;
    }

    /// <summary>
    /// 使用 TLS 中间件
    /// </summary>
    public static ListenOptions UseTls(this ListenOptions listen)
    {
        var certService = listen.ApplicationServices.GetRequiredService<IX509CertService>();
        listen.Use(next => context => TlsInvadeMiddleware.InvokeAsync(next, context));
        listen.UseHttps(new TlsHandshakeCallbackOptions()
        {
            OnConnection = OnConnectionAsync,
        });
        listen.Use(next => context => TlsRestoreMiddleware.InvokeAsync(next, context));
        return listen;

        async ValueTask<SslServerAuthenticationOptions> OnConnectionAsync(TlsHandshakeCallbackContext context)
        {
            X509Certificate? serverCert = null;
            try
            {
                serverCert = await certService.GetServerCertificateAsync(context.ClientHelloInfo, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            SslServerAuthenticationOptions o = new()
            {
                ServerCertificate = serverCert,
            };
            return o;
        }
    }
}
