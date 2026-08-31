using AigioL.Common.Net.ReverseProxy.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Text;

namespace AigioL.Common.Net.ReverseProxy;

sealed partial class ReverseProxyWebApp : IReverseProxyWebApp, IAsyncDisposable
{
    WebApplication? app;

    readonly string[] allowedHosts = ["*",];
    readonly IReverseProxyWebAppLifecycle lifecycle;

    public ReverseProxyWebApp()
    {
        lifecycle = new ReverseProxyWebAppLifecycle(this);
    }

    /// <inheritdoc/>
    public async Task<ReverseProxyWebAppStartErrCode> StartAsync()
    {
        try
        {
#if WINDOWS
            if (!IcuTest(allowedHosts))
            {
                return ReverseProxyWebAppStartErrCode.IcuTestFail;
            }
#endif
            var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions()
            {
                ApplicationName = "AigioL.Common.Net.ReverseProxy",
            });

            //builder.Logging.AddProvider(AccConsoleLogProvider.Instance);

            builder.Services.AddSingleton(lifecycle);
            builder.Services.Configure<HostFilteringOptions>(o =>
            {
                o.AllowEmptyHosts = true;
                o.AllowedHosts = allowedHosts;
            });

            //builder.Host.UseNLog();
            StartupConfigureServices(builder.Services);
            builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(3d)); // 指定等待 Web 主机关闭的时间量
            builder.WebHost.UseKestrelCore();
            builder.WebHost.UseKestrelHttpsConfiguration(); // HTTPS
            //if (HttpVersionHelper.DefaultListenHttpProtocols.HasFlag(HttpProtocols.Http3))
            //{
            //    // 本地到本地反代不需要 QUIC/HTTP3，见 DefaultListenHttpProtocols
            //    builder.WebHost.UseQuic(static options =>
            //    {
            //        // Configure server defaults to match client defaults.
            //        // https://github.com/dotnet/runtime/blob/a5f3676cc71e176084f0f7f1f6beeecd86fbeafc/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/ConnectHelper.cs#L118-L119
            //        options.DefaultStreamErrorCode = 268/*(long)Http3ErrorCode.RequestCancelled*/;
            //        options.DefaultCloseErrorCode = 256/*(long)Http3ErrorCode.NoError*/;
            //    }); // HTTP/3
            //}
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.AddServerHeader = false;
                options.RequestHeaderEncodingSelector = RequestHeaderEncodingSelector;
                options.ResponseHeaderEncodingSelector = ResponseHeaderEncodingSelector;
                options.NoLimit();
#if WINDOWS
                //options.ListenSshReverseProxy();
                //options.ListenGitReverseProxy();
#endif

                //if (ProxyMode is ProxyMode.System or ProxyMode.PAC or ProxyMode.VPN)
                //{
                //    options.ListenHttpProxy();
                //}
                //else
                //{
                //    options.ListenHttpsReverseProxy();
                //    if (EnableHttpProxyToHttps)
                //        options.ListenHttpReverseProxy();
                //}
            });

            app = builder.Build();
            app.UseHostFiltering();
            StartupConfigure(app);
        }
        catch
        {
        }
        return ReverseProxyWebAppStartErrCode.Ok;
    }

    /// <inheritdoc cref="KestrelServerOptions.RequestHeaderEncodingSelector"/>
    Encoding? RequestHeaderEncodingSelector(string header)
    {
        return Encoding.UTF8;
    }

    /// <inheritdoc cref="KestrelServerOptions.ResponseHeaderEncodingSelector"/>
    Encoding? ResponseHeaderEncodingSelector(string header)
    {
        return Encoding.UTF8;
    }

    void StartupConfigureServices(IServiceCollection services)
    {
    }

    void StartupConfigure(IApplicationBuilder app)
    {
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {

    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {

    }
}

file sealed class ReverseProxyWebAppLifecycle(ReverseProxyWebApp app) : IReverseProxyWebAppLifecycle
{
    void IReverseProxyWebAppLifecycle.Stop() => Task.Factory.StartNew(async () =>
    {
        await app.StopAsync();
    });
}