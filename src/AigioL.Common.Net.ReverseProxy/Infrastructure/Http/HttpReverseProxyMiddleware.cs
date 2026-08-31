using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Text;
using Yarp.ReverseProxy.Forwarder;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;
using static AigioL.Common.Net.ReverseProxy.Infrastructure.Binary.ScriptInjectHelper;
using static AigioL.Common.Net.ReverseProxy.Infrastructure.Http.LoggerMessages;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// HTTP 反向代理中间件
/// </summary>
sealed partial class HttpReverseProxyMiddleware
{
    readonly IHttpForwarder forwarder;
    readonly IReverseProxyHttpMessageInvokerFactory factory;
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly FixedServersDnsResolver fixedServersDnsResolver;
    readonly ILogger logger;

#pragma warning disable IDE0290 // 使用主构造函数
    public HttpReverseProxyMiddleware(
#pragma warning restore IDE0290 // 使用主构造函数
        IHttpForwarder forwarder,
        IReverseProxyHttpMessageInvokerFactory factory,
        IReverseProxyConfig reverseProxyConfig,
        FixedServersDnsResolver fixedServersDnsResolver,
        ILogger<HttpReverseProxyMiddleware> logger)
    {
        this.forwarder = forwarder;
        this.factory = factory;
        this.reverseProxyConfig = reverseProxyConfig;
        this.fixedServersDnsResolver = fixedServersDnsResolver;
        this.logger = logger;
    }

    static readonly RecyclableMemoryStreamManager m = new();

    /// <summary>
    /// 处理请求
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var isScriptInject = reverseProxyConfig.TryGetScriptConfig(context.Request, out var scriptConfigs);

        var rspBodyRaw = context.Response.Body; // 原始响应流
        if (isScriptInject)
        {
            var rspBody = m.GetStream();
            context.Response.RegisterForDisposeAsync(rspBody);
            context.Response.Body = rspBody;
        }

        if (!TryGetDomainConfig(context.Request, out var domainConfig))
        {
            if (!HttpNoProxy.IsNoProxy(reverseProxyConfig.SecondLevelWebProxy)) // 如果启用了二级 Web 代理
            {
                var h = factory.CreateHttpMessageHandler(KeyGlobalProxy, DomainConfig.Default);
                var destinationPrefix = GetYarpDestinationPrefix(context.Request.Scheme, context.Request.Host);
                var forwarderRequestConfig = new ForwarderRequestConfig()
                {
                    Version = context.Request.GetHttpVersion(),
                };
                var error = await forwarder.SendAsync(context, destinationPrefix, h, forwarderRequestConfig, HttpTransformer.Empty);
                if (error != ForwarderError.None)
                {
                    await HandleErrorAsync(context, error);
                    return;
                }
            }
            else
            {
                await next(context);
            }
            return;
        }

        if (DomainConfig.Default.Equals(domainConfig) && !reverseProxyConfig.OnlyEnableProxyScript)
        {
            // 部分运营商将奇怪的域名解析到 127.0.0.1 再此排除这些不支持的代理域名
            IPAddress? hostIp = null;
            if (context.Request.Host.HasValue)
            {
                // 大多默认配置为系统 DNS，这里用预设的固定值 DNS 服务器 fixedServersDnsResolver 再尝试一次
                var ipResult = await fixedServersDnsResolver.ResolveAddressesAsync(context.Request.Host.Value, cancellationToken: context.RequestAborted);
                if (ipResult.Result.Records.Count != 0)
                {
                    hostIp = ipResult.Result.Records[0].Address;
                }
            }
            if (hostIp == null || IPAddress.IsLoopback(hostIp))
            {
                context.Response.ContentType = "text/plain; charset=utf-8";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                context.Response.BodyWriter.Write("域名 "u8);
                context.Response.BodyWriter.WriteHostString(context.Request.Host);
                context.Response.BodyWriter.Write(" 可能已经被 DNS 污染，如果域名为本机域名，请解析为非回环 IP"u8);
                context.Response.BodyWriter.Write("\r\n"u8);

                context.Response.BodyWriter.Write("请求地址："u8);
                context.Request.WriteDisplayUrl();
                context.Response.BodyWriter.Write("\r\n"u8);

                await context.Response.BodyWriter.FlushAsync(context.RequestAborted);
                return;
            }
        }

        if (domainConfig.Items != null && domainConfig.Items.Count != 0)
        {
            domainConfig = RecursionMatchDomainConfig(context.Request, domainConfig);
        }

        if (domainConfig.Response != null)
        {
            await domainConfig.Response.ExecuteAsync(context);
            return;
        }
        else
        {
            if (string.Equals(Uri.UriSchemeHttp, context.Request.Scheme, StringComparison.InvariantCultureIgnoreCase) && reverseProxyConfig.EnableHttpProxyToHttps)
            {
                var location = context.Request.GetDisplayUrlHttps();
                context.Response.Redirect(location);
                return;
            }

            var destination = domainConfig.Destination;
            if (destination?.AbsoluteUri.Contains(TemplateStringVarPrefix, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                var rawUrlLength = destination.AbsoluteUri.Contains(TemplateStringVarUri) ? context.Request.GetRawUrlLength() : 0;
                char[]? array = null;
                Span<char> rawUrl = rawUrlLength == default ? default : (rawUrlLength <= StackallocCharThreshold ?
                    stackalloc char[StackallocCharThreshold] :
                    (array = ArrayPool<char>.Shared.Rent(rawUrlLength)).AsSpan(0, rawUrlLength));
                try
                {
                    var destinationBuilder = new StringBuilder(destination.AbsoluteUri);
                    destinationBuilder.Replace(TemplateStringVarDomain, context.Request.Host.GetHost());

                    if (rawUrlLength != 0)
                    {
                        context.Request.WriteRawUrl(rawUrl);
                        destinationBuilder.Replace(TemplateStringVarUri, rawUrl);
                    }

                    destination = new Uri(destinationBuilder.ToString(), UriKind.Absolute);
                }
                finally
                {
                    if (array is not null)
                    {
                        ArrayPool<char>.Shared.Return(array);
                    }
                }
            }

            var destinationPrefix = GetYarpDestinationPrefix(context.Request.Scheme, context.Request.Host, destination);
            var h = factory.CreateHttpMessageHandler(context.Request.Host.Host, domainConfig);
            if (!string.IsNullOrEmpty(domainConfig.UserAgent))
            {
                var newUA = domainConfig.UserAgent.Replace(TemplateStringVarOrigin, context.Request.Headers.UserAgent, StringComparison.InvariantCultureIgnoreCase);
                context.Request.Headers.UserAgent = newUA;
            }

            ForwarderRequestConfig forwarderRequestConfig;

            if (domainConfig.IsServerSideProxy(context.Request, out var forwarderRequestConfigTemp))
            {
                forwarderRequestConfig = forwarderRequestConfigTemp;
            }
            else
            {
                forwarderRequestConfig = new()
                {
                    Version = context.Request.GetHttpVersion(),
                };
            }

            var error = await forwarder.SendAsync(context, destinationPrefix, h, forwarderRequestConfig, HttpTransformer.Empty);
            if (error != ForwarderError.None)
            {
                await HandleErrorAsync(context, error);
                return;
            }
            else if (isScriptInject)
            {
                await HandleScriptInjectAsync(context, scriptConfigs, rspBodyRaw);
                return;
            }
        }
    }

    /// <summary>
    /// 递归匹配子域名配置
    /// </summary>
    static IDomainConfig RecursionMatchDomainConfig(HttpRequest req, IDomainConfig domainConfig)
    {
        if (domainConfig.Items != null && domainConfig.Items.Count != 0)
        {
            var len = req.GetDisplayUrlLength();
            char[]? array = null;
            Span<char> displayUrl = len <= StackallocCharThreshold ?
                stackalloc char[StackallocCharThreshold] :
                (array = ArrayPool<char>.Shared.Rent(len)).AsSpan(0, len);
            try
            {
                req.WriteDisplayUrl(displayUrl);
                var matchedConfig = RecursionMatchDomainConfigCore(displayUrl, domainConfig.Items);
                if (matchedConfig != null)
                {
                    return matchedConfig;
                }
            }
            finally
            {
                if (array is not null)
                {
                    ArrayPool<char>.Shared.Return(array);
                }
            }
        }
        return domainConfig;
    }

    static IDomainConfig? RecursionMatchDomainConfigCore(ReadOnlySpan<char> displayUrl, IReadOnlyDictionary<DomainPattern, IDomainConfig> items)
    {
        foreach (var it in items)
        {
            var isMatch = it.Key.IsMatch(displayUrl);
            if (isMatch)
            {
                if (it.Value != null && it.Value.Items != null && it.Value.Items.Count != 0)
                {
                    return RecursionMatchDomainConfigCore(displayUrl, it.Value.Items);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 尝试获取域名配置
    /// </summary>
    bool TryGetDomainConfig(HttpRequest req, [MaybeNullWhen(false)] out IDomainConfig value)
    {
        value = null;

        if (!reverseProxyConfig.OnlyEnableProxyScript && reverseProxyConfig.TryGetDomainConfig(req, out value))
        {
            return true;
        }

        // 未配置的域名，但仍然被解析到本机 IP 的域名
        if (IsDomain(req.Host.GetHost()))
        {
            LogWarnDNS(logger, req.Host);
            value = DomainConfig.Default;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 是否为域名
    /// </summary>
    static bool IsDomain(ReadOnlySpan<char> host) => !IPAddress2.TryParse(host, out _) && host.Contains('.');

    /// <summary>
    /// 获取 YARP 代理请求的 URI 前缀
    /// </summary>
    string GetYarpDestinationPrefix(string scheme, HostString host)
    {
        return $"{scheme}://{host}/";
    }

    /// <summary>
    /// 获取 YARP 代理请求的 URI 前缀
    /// </summary>
    string GetYarpDestinationPrefix(string scheme, HostString host, Uri? destination)
    {
        if (destination == null)
        {
            return GetYarpDestinationPrefix(scheme, host);
        }
        var u = new Uri(new Uri($"{scheme}://{host}/", UriKind.Absolute), destination);
        return u.ToString();
    }

    /// <summary>
    /// 处理 <see cref="ForwarderError"/> 错误，如果有错误，则将错误信息写入响应流，否则不做任何处理
    /// </summary>
    static async Task HandleErrorAsync(HttpContext context, ForwarderError error)
    {
        if (error == ForwarderError.None)
        {
            return;
        }
        else
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            context.Response.BodyWriter.Write("YARP 转发到目的服务器时发生错误："u8);
            context.Response.BodyWriter.Write((int)error);
            context.Response.BodyWriter.Write("\r\n"u8);

            context.Response.BodyWriter.Write("请求地址："u8);
            var displayUrl = context.Request.GetDisplayUrl();
            context.Response.BodyWriter.Write(displayUrl);
            context.Response.BodyWriter.Write("\r\n"u8);

            var ex = context.GetForwarderErrorFeature()?.Exception;
            if (ex != null)
            {
                context.Response.BodyWriter.Write(ex.ToString());
            }
            await context.Response.BodyWriter.FlushAsync(context.RequestAborted);
        }
    }

    /// <summary>
    /// 处理脚本注入内容
    /// </summary>
    async Task HandleScriptInjectAsync(HttpContext context, IReadOnlyCollection<IScriptConfig>? scripts, Stream rspBodyRaw)
    {
        async Task ResetBody()
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            context.Response.ContentLength = context.Response.Body.Length;
            await context.Response.Body.CopyToAsync(rspBodyRaw);
            context.Response.Body = rspBodyRaw;
        }

        //async Task SetBodyAsync(Stream stream, bool leaveOpen = false)
        //{
        //    Stream? rspBodyTemp = null;
        //    try
        //    {
        //        context.Response.ContentLength = stream.Length;
        //        stream.Seek(0, SeekOrigin.Begin);
        //        await stream.CopyToAsync(rspBodyRaw, context.RequestAborted);
        //        rspBodyTemp = context.Response.Body;
        //        context.Response.Body = rspBodyRaw;
        //    }
        //    finally
        //    {
        //        if (!leaveOpen)
        //        {
        //            if (rspBodyTemp != null)
        //            {
        //                await rspBodyTemp.DisposeAsync();
        //            }
        //        }
        //    }
        //}

        if (!(scripts != null && scripts.Count != 0) ||
            context.Request.Method != HttpMethods.Get ||
            context.Response.StatusCode != StatusCodes.Status200OK ||
            context.Response.ContentType == null ||
            !context.Response.ContentType.Contains(MediaTypeNames.Text.Html, StringComparison.InvariantCultureIgnoreCase))
        {
            // 只针对 HTML 页面启用脚本时，通过 Content-Type 判断是否为 HTML 页面，如果不是，则不注入脚本
            await ResetBody();
            return;
        }

        if (reverseProxyConfig.IsOnlyWorkSteamBrowser &&
            context.Request.Headers.UserAgent.Contains("Valve Steam", StringComparer.InvariantCulture) == false)
        {
            // 只针对 Steam 内置浏览器启用脚本时，通过 User-Agent 判断是否为 Steam 内置浏览器，如果不是，则不注入脚本
            await ResetBody();
            return;
        }

        if (!StringValues.IsNullOrEmpty(context.Response.Headers.ContentSecurityPolicy))
        {
            // https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Guides/CSP
            context.Response.Headers.Remove(HeaderNames.ContentSecurityPolicy);
        }

        try
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Reference/Headers/Content-Encoding
            var contentEncoding = context.Response.Headers.GetContentEncoding().FirstOrDefault(); // 响应头通常只有一个值
            using var bodyDecompress = contentEncoding.GetCompressionStream(context.Response.Body, CompressionMode.Decompress, true); // 根据响应头的 Content-Encoding 解压响应体

            RecyclableMemoryStream? bodyDecompressMemory = null;
            var usingBodyDecompressMemory = false;
            try
            {
                if (bodyDecompress != null)
                {
                    // 创建临时内存流，将 CompressionStream 中数据拷贝过去，实现解压数据
                    bodyDecompressMemory = m.GetStream();
                    usingBodyDecompressMemory = true;
                    await bodyDecompress.CopyToAsync(bodyDecompressMemory, context.RequestAborted);
                }
                else if (context.Response.Body is RecyclableMemoryStream bodyRecyclableMemoryStream)
                {
                    // CompressionStream 为 null，表示没有压缩，直接使用原流，避免复制
                    bodyDecompressMemory = bodyRecyclableMemoryStream;
                }
                else
                {
                    // CompressionStream 为 null，但是原流类型不确定，创建临时内存流，拷贝一份，以便写入和重置位置
                    bodyDecompressMemory = m.GetStream();
                    usingBodyDecompressMemory = true;
                    await context.Response.Body.CopyToAsync(bodyDecompressMemory, context.RequestAborted);
                }
                bodyDecompressMemory.Position = 0;

                var encoding = context.Response.GetEncoding();

                var isGithubHost = IsGitHubHost(context.Request.Host);
                var isFindPos = isGithubHost
                    ? FindScriptInjectInsertPositionForGitHub(bodyDecompressMemory, encoding, out var position)
                    : FindScriptInjectInsertPosition(bodyDecompressMemory, encoding, out position);

                if (isFindPos)
                {
                    // 与旧版本的变更：HTML 内容不再根据原始响应头中的 ContentEncoding 进行压缩，直接返回未压缩的内容，避免再进行压缩导致的不必要的内存分配
                    await WriteUtf8HtmlAsync(rspBodyRaw, position, bodyDecompressMemory.GetReadOnlySequence(), scripts, context.RequestAborted);
                    if (context.Response.Body != rspBodyRaw)
                    {
                        await context.Response.Body.DisposeAsync();
                        context.Response.Body = rspBodyRaw;
                    }
                    context.Response.Headers.Remove(HeaderNames.ContentType); // 内容未压缩
                }
            }
            finally
            {
                if (usingBodyDecompressMemory && bodyDecompressMemory != null)
                {
                    await bodyDecompressMemory.DisposeAsync();
                }
            }

        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogErrorHandleScriptInject(logger, ex,
                context.Request.Scheme,
                context.Request.Host,
                context.Request.PathBase,
                context.Request.Path,
                context.Request.QueryString);
            await ResetBody();
        }
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "域名 {host} 可能已经被 DNS 污染，如果域名为本机域名，请解析为非回环 IP")]
    internal static partial void LogWarnDNS(ILogger logger, HostString host);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "处理脚本注入内容失败，url: {scheme}://{host}{pathBase}{path}{queryString}")]
    internal static partial void LogErrorHandleScriptInject(ILogger logger, Exception? exception, string? scheme, HostString host, PathString pathBase, PathString path, QueryString queryString);
}