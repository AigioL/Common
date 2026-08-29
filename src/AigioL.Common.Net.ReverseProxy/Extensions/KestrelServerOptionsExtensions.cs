using AigioL.Common.Net.ReverseProxy.Infrastructure.Http;
using AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System.Runtime.CompilerServices;
using static Microsoft.AspNetCore.Hosting.LoggerMessages_KestrelOptEx;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Hosting;

/// <summary>
/// <see cref="KestrelServerOptions"/> 的扩展方法
/// </summary>
public static partial class KestrelServerOptionsExtensions
{
    const string TAG = "KestrelOptEx";
    const HttpProtocols DefHttpProtocols = HttpProtocols.Http1AndHttp2AndHttp3;

    /// <summary>
    /// 配置 KestrelServer 为无限制（移除默认的请求正文允许的最大大小、最小响应数据速率、最小请求正文数据速率）
    /// </summary>
    public static void NoLimit(this KestrelServerOptions options)
    {
        // https://learn.microsoft.com/zh-cn/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits#properties
        options.Limits.MaxRequestBodySize = null;
        options.Limits.MinResponseDataRate = null;
        options.Limits.MinRequestBodyDataRate = null;
    }

    /// <summary>
    /// 根据传入的 IP 地址识别调用不同的 <see cref="KestrelServerOptions.Listen(IPAddress, int, Action{ListenOptions})"/>，使用不同的 <see cref="ListenOptions"/> 派生类，例如 <see cref="AnyIPListenOptions"/>、<see cref="LocalhostListenOptions"/>
    /// <list type="bullet">
    /// <item>当 IP 地址是回环地址 <see cref="IPAddress.IsLoopback(IPAddress)"/> 时，调用 <see cref="KestrelServerOptions.ListenLocalhost(int, Action{ListenOptions})"/></item>
    /// <item>当 IP 地址是任意地址 <see cref="IPAddress.Any"/> 或 <see cref="IPAddress.IPv6Any"/> 时，调用 <see cref="KestrelServerOptions.ListenAnyIP(int, Action{ListenOptions})"/></item>
    /// <item>否则，调用 <see cref="KestrelServerOptions.Listen(IPAddress, int, Action{ListenOptions})"/></item>
    /// </list>
    /// </summary>
    static void ListenByIpAutoDetection(this KestrelServerOptions options, IPAddress address, int port, Action<ListenOptions> configure)
    {
        if (IPAddress.IsLoopback(address))
        {
            // https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Servers/Kestrel/Core/src/LocalhostListenOptions.cs
            options.ListenLocalhost(port, configure);
        }
        else if (IPAddress.Any.Equals(address) || IPAddress.IPv6Any.Equals(address))
        {
            // https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Servers/Kestrel/Core/src/AnyIPListenOptions.cs
            options.ListenAnyIP(port, configure);
        }
        else
        {
            options.Listen(address, port, configure);
        }
    }

    /// <summary>
    /// 配置 KestrelServer 监听 HTTP 代理服务
    /// </summary>
    public static void ListenHttpProxy(this KestrelServerOptions options, IPAddress ip, int port, HttpProtocols protocols = DefHttpProtocols)
    {
        var logger = options.GetLogger();
        Exception? lastException = null;
        try
        {
            //if (!IReverseProxyConfig.IsAvailableTcp(httpProxyPort))
            //{
            //    throw new ApplicationException(
            //        $"TCP port {httpProxyPort} is already occupied by other processes.");
            //}

            options.ListenByIpAutoDetection(ip, port, listen =>
            {
                listen.Protocols = protocols;
                var proxyMiddleware = options.ApplicationServices.GetRequiredService<HttpProxyMiddleware>();
                var tunnelMiddleware = options.ApplicationServices.GetRequiredService<TunnelMiddleware>();

                listen.UseFlowAnalyze();
                listen.Use(next => context => proxyMiddleware.InvokeAsync(next, context));
                listen.UseTls();
                listen.Use(next => context => tunnelMiddleware.InvokeAsync(next, context));
            });
        }
        catch (Exception ex)
        {
            lastException = ex;
        }
        finally
        {
            if (lastException == null)
            {
                LogInfoListenedHttp(logger, ip, port);
            }
            else
            {
                LogErrorListenedHttp(logger, lastException, ip, port);
            }
        }
    }

    #region ListenReverseProxy

    /// <summary>
    /// 配置 KestrelServer 监听 SSH 反向代理服务
    /// </summary>
    public static void ListenSshReverseProxy(this KestrelServerOptions options, IPAddress ip, int port)
    {
        var logger = options.GetLogger();
        Exception? lastException = null;
        try
        {
            options.ListenByIpAutoDetection(ip, port, listen =>
            {
                listen.UseFlowAnalyze();
                listen.UseConnectionHandler<GitHubSshReverseProxyHandler>();
            });
        }
        catch (Exception ex)
        {
            lastException = ex;
        }
        finally
        {
            if (lastException == null)
            {
                LogInfoListenedSshReverseProxy(logger, ip, port);
            }
            else
            {
                LogErrorListenedSshReverseProxy(logger, lastException, ip, port);
            }
        }
    }

    /// <summary>
    /// 配置 KestrelServer 监听 Git 反向代理服务
    /// </summary>
    public static void ListenGitReverseProxy(this KestrelServerOptions options, IPAddress ip, int port)
    {
        var logger = options.GetLogger();
        Exception? lastException = null;
        try
        {
            options.ListenByIpAutoDetection(ip, port, listen =>
            {
                listen.UseFlowAnalyze();
                listen.UseConnectionHandler<GitHubGitReverseProxyHandler>();
            });
        }
        catch (Exception ex)
        {
            lastException = ex;
        }
        finally
        {
            if (lastException == null)
            {
                LogInfoListenedGitReverseProxy(logger, ip, port);
            }
            else
            {
                LogErrorListenedGitReverseProxy(logger, lastException, ip, port);
            }
        }
    }

    /// <summary>
    /// 配置 KestrelServer 监听 HTTP 反向代理服务
    /// </summary>
    public static void ListenHttpReverseProxy(this KestrelServerOptions options, IPAddress ip, int port)
    {
        var logger = options.GetLogger();
        Exception? lastException = null;
        try
        {
            options.Listen(ip, port);
        }
        catch (Exception ex)
        {
            lastException = ex;
        }
        finally
        {
            if (lastException == null)
            {
                LogInfoListenedHttpReverseProxy(logger, ip, port);
            }
            else
            {
                LogErrorListenedHttpReverseProxy(logger, lastException, ip, port);
            }
        }
    }

    /// <summary>
    /// 配置 KestrelServer 监听 HTTPS 反向代理
    /// </summary>
    public static void ListenHttpsReverseProxy(this KestrelServerOptions options, IPAddress ip, int port, HttpProtocols protocols = DefHttpProtocols)
    {
        var logger = options.GetLogger();
        Exception? lastException = null;
        try
        {
            options.Listen(ip, port, listen =>
            {
                listen.Protocols = protocols;
                listen.UseFlowAnalyze();
                listen.UseTls();
            });
        }
        catch (Exception ex)
        {
            lastException = ex;
        }
        finally
        {
            if (lastException == null)
            {
                LogInfoListenedHttpsReverseProxy(logger, ip, port);
            }
            else
            {
                LogErrorListenedHttpsReverseProxy(logger, lastException, ip, port);
            }
        }
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ILogger GetLogger(this KestrelServerOptions kestrel)
    {
        var loggerFactory = kestrel.ApplicationServices.GetRequiredService<ILoggerFactory>();
        return loggerFactory.CreateLogger(TAG);
    }
}

internal static partial class LoggerMessages_KestrelOptEx
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "已监听 ssh://{ip}:{port}，SSH 反向代理服务已启动")]
    internal static partial void LogInfoListenedSshReverseProxy(ILogger logger, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "监听失败 ssh://{ip}:{port}，SSH 反向代理服务未启动")]
    internal static partial void LogErrorListenedSshReverseProxy(ILogger logger, Exception exception, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "已监听 git://{ip}:{port}，Git 反向代理服务启动")]
    internal static partial void LogInfoListenedGitReverseProxy(ILogger logger, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "监听失败 git://{ip}:{port}，Git 反向代理服务已未启动")]
    internal static partial void LogErrorListenedGitReverseProxy(ILogger logger, Exception exception, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "已监听 http://{ip}:{port}，HTTP 代理服务已启动")]
    internal static partial void LogInfoListenedHttp(ILogger logger, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "监听失败 http://{ip}:{port}，HTTP 代理服务未启动")]
    internal static partial void LogErrorListenedHttp(ILogger logger, Exception exception, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "已监听 http://{ip}:{port}，HTTP 反向代理服务已启动")]
    internal static partial void LogInfoListenedHttpReverseProxy(ILogger logger, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "监听失败 http://{ip}:{port}，HTTP 反向代理服务未启动")]
    internal static partial void LogErrorListenedHttpReverseProxy(ILogger logger, Exception exception, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "已监听 https://{ip}:{port}，HTTPS 反向代理服务已启动")]
    internal static partial void LogInfoListenedHttpsReverseProxy(ILogger logger, IPAddress ip, int port);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "监听失败 https://{ip}:{port}，HTTPS 反向代理服务未启动")]
    internal static partial void LogErrorListenedHttpsReverseProxy(ILogger logger, Exception exception, IPAddress ip, int port);
}