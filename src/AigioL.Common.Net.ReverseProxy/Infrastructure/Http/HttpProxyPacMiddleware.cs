using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// HTTP 代理自动配置（PAC）策略中间件
/// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Guides/Proxy_servers_and_tunneling/Proxy_Auto-Configuration_PAC_file</para>
/// </summary>
sealed class HttpProxyPacMiddleware(IReverseProxyConfig reverseProxyConfig)
{
    readonly IReverseProxyConfig reverseProxyConfig = reverseProxyConfig;

    /// <summary>
    /// 处理请求
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // HTTP 请求经过了 HttpProxy 中间件
        var proxyFeature = context.Features.Get<IHttpProxyFeature>();
        if (proxyFeature != null && proxyFeature.ProxyProtocol == ProxyProtocol.None)
        {
            var domainPatterns = reverseProxyConfig.GetDomainPatterns();
            context.Response.ContentType = "application/x-ns-proxy-autoconfig";
            context.Response.Headers.Append("Content-Disposition", "attachment;filename=proxy.pac");
            WriterProxyPac(context.Response.BodyWriter, context.Request.Host, domainPatterns);
        }
        else
        {
            await next(context);
        }
    }

    /// <summary>
    /// 写入代理自动配置（PAC）文件
    /// </summary>
    static void WriterProxyPac(
        PipeWriter writer,
        HostString proxyHost,
        params IEnumerable<DomainPattern> domainPatterns)
    {
        // 直接使用 UTF-8 字节写入字符到 Pipe 中
        var newLine = "\r\n"u8; // JS 换行符统一按 Windows 风格
        // https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Guides/Proxy_servers_and_tunneling/Proxy_Auto-Configuration_PAC_file#%E7%A4%BA%E4%BE%8B_1
        writer.Write("function FindProxyForURL(url, host){"u8);
        writer.Write(newLine);
        writer.Write("    var pac = 'PROXY "u8);
        writer.WriteHostString(proxyHost);
        writer.Write("';"u8);
        writer.Write(newLine);
        foreach (var domains in domainPatterns)
        {
            var domainsSpan = domains.AsSpan();
            var split = domainsSpan.Split(GeneralSeparator);
            while (split.MoveNext())
            {
                var domain = domainsSpan[split.Current];
                if (!domain.IsWhiteSpace())
                {
                    writer.Write("    if (shExpMatch(host, '"u8);
                    writer.Write(domain);
                    writer.Write("')) return pac;"u8);
                    writer.Write(newLine);
                }
            }
        }
        writer.Write("    return 'DIRECT';"u8);
        writer.Write(newLine);
        writer.Write("}"u8);
        writer.Write(newLine);
    }
}