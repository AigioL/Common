using AigioL.Common.Net.ReverseProxy.Internals.Configuration;
using AigioL.Common.Net.ReverseProxy.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace AigioL.Common.Net.ReverseProxy.Internals.Http;

/// <summary>
/// 本地 HTTP 代理策略中间件
/// </summary>
sealed class HttpLocalRequestMiddleware(
    IReverseProxyConfig reverseProxyConfig,
    ICookieHttpClientFactory cookieHttpClientFactory)
{
    readonly IReverseProxyConfig reverseProxyConfig = reverseProxyConfig;
    readonly ICookieHttpClientFactory cookieHttpClientFactory = cookieHttpClientFactory;

    /// <summary>
    /// 处理请求
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
#pragma warning disable IDE0018 // 内联变量声明
        int localScriptId;
#pragma warning restore IDE0018 // 内联变量声明
        if (TryGetInjectScriptLocalId(context.Request.Path, out localScriptId))
        {
            await HandleScriptRequestAsync(context, localScriptId);
            return;
        }

        if (HttpMethods.IsOptions(context.Request.Method) &&
            context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
        {
            // https://wicg.github.io/private-network-access
            context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
        }

        if (context.Request.Host.Host.Equals(LocalDomain, StringComparison.InvariantCultureIgnoreCase))
        {
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                context.Response.Headers.AccessControlAllowOrigin = context.Request.Headers.Origin.Count == 0 ? "*" : context.Request.Headers.Origin;
                context.Response.Headers.AccessControlAllowHeaders = "*";
                context.Response.Headers.AccessControlAllowMethods = "*";
                context.Response.Headers.AccessControlAllowCredentials = "true";

                await context.Response.WriteAsync(string.Empty);
                return;
            }

            var type = context.Request.Headers["requestType"];
            switch (type)
            {
                case "status":
                    context.Response.ContentType = "text/plain;charset=UTF-8";
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    context.Response.Body.Write("OK"u8);
                    return;
                case "xhr":
                    await HandleHttpRequestAsync(context);
                    return;
                default: // 默认处理脚本匹配
                    if (context.Request.Path.Value != null)
                    {
                        var pathValueSpan = context.Request.Path.Value.AsSpan();
                        pathValueSpan = pathValueSpan.Trim('/');
                        if (int.TryParse(pathValueSpan, out localScriptId) && localScriptId > 0)
                        {
                            await HandleScriptRequestAsync(context, localScriptId);
                            return;
                        }
                    }
                    await Handle404NotFoundAsync(context);
                    return;
            }
        }

        await next(context);
    }

    /// <summary>
    /// 尝试从请求路径中获取注入脚本的本地 Id
    /// </summary>
    /// <param name="path">请求路径，例如 <see cref="HttpRequest.Path"/></param>
    /// <param name="localScriptId">脚本本地 Id</param>
    /// <returns>如果成功获取脚本本地 Id，则返回 true；否则返回 false</returns>
    static bool TryGetInjectScriptLocalId(PathString path, out int localScriptId)
    {
        localScriptId = default;
        var pathValue = path.Value;
        if (string.IsNullOrEmpty(pathValue))
        {
            return false;
        }

        var pathValueSpan = pathValue.AsSpan(); // 使用 Span<char> 来避免不必要的字符串分配

        if (!pathValueSpan.StartsWith(InjectScriptPathPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        var idPart = pathValueSpan[InjectScriptPathPrefix.Length..];
        if (!idPart.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        idPart = idPart[..^3];
        var r = int.TryParse(idPart, out localScriptId) && localScriptId > 0;
        return r;
    }

    async Task HandleScriptRequestAsync(HttpContext context, int localScriptId)
    {
        if (reverseProxyConfig.TryGetScriptContent(localScriptId, out var content))
        {
            context.Response.Headers.ContentType = "application/javascript;charset=UTF-8";
            await context.Response.WriteAsync(content, context.RequestAborted);
            return;
        }

        await Handle404NotFoundAsync(context);
    }

    static bool TryParse(string method, [NotNullWhen(true)] out HttpMethod? httpMethod)
    {
        httpMethod = method switch
        {
            "GET" => HttpMethod.Get,
            "PUT" => HttpMethod.Put,
            "POST" => HttpMethod.Post,
            "DELETE" => HttpMethod.Delete,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            "TRACE" => HttpMethod.Trace,
            "PATCH" => HttpMethod.Patch,
            "CONNECT" => HttpMethod.Connect,
            _ => TryParseCore(method),
        };
        return httpMethod != null;

        static HttpMethod? TryParseCore(string method)
        {
            try
            {
                return new HttpMethod(method.ToUpperInvariant());
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 处理脚本所需要的 HTTP 请求
    /// </summary>
    async Task HandleHttpRequestAsync(HttpContext context)
    {
        try
        {
            var queryString = context.Request.QueryString.Value;
            if (string.IsNullOrEmpty(queryString))
            {
                return;
            }

            const string queryStringPrefix = "?request=";
            var queryStringSpan = queryString.AsSpan();
            if (queryStringSpan.StartsWith(queryStringPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                queryStringSpan = queryStringSpan[queryStringPrefix.Length..];
            }

            var requestUriString = Uri.UnescapeDataString(queryStringSpan);
            if (!Uri.TryCreate(requestUriString, UriKind.Absolute, out var requestUri))
            {
                return;
            }

            if (!TryParse(context.Request.Method, out var method))
            {
                return;
            }

            context.Response.Headers.AccessControlAllowOrigin = context.Request.Headers.Origin.Count == 0 ? "*" : context.Request.Headers.Origin;
            context.Response.Headers.AccessControlAllowHeaders = "*";
            context.Response.Headers.AccessControlAllowMethods = "*";
            context.Response.Headers.AccessControlAllowCredentials = "true";

            //if (cookie != null)
            //    context.Response.Cookies.Append(cookie);

            if (context.Request.ContentType != null)
            {
                context.Response.Headers.ContentType = context.Request.ContentType;
            }

            await SendAsync(context, method, requestUri);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/plain;charset=UTF-8";
            await context.Response.WriteAsync(ex.ToString(), context.RequestAborted);
        }
    }

    async Task SendAsync(HttpContext context, HttpMethod method, Uri requestUri)
    {
        var c = cookieHttpClientFactory.CreateClient();

        using var req = new HttpRequestMessage
        {
            RequestUri = requestUri,
            Method = method,
        };
        if (context.Request.ContentLength > 0 &&
            !HttpMethods.IsGet(method.Method) &&
            !HttpMethods.IsDelete(method.Method))
        {
            req.Content = new StreamContent(context.Request.Body);
        }
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.EndsWith(HttpHeader_SteamTool, StringComparison.InvariantCultureIgnoreCase))
            {
                if (header.Key.StartsWith(HttpHeaderCookie, StringComparison.InvariantCultureIgnoreCase))
                {
                    var cookieContainer = cookieHttpClientFactory.GetCookieContainer();
                    var cookie = new Cookie
                    {
                        CommentUri = requestUri,
                        Domain = requestUri.Host,
                        Value = header.Value,
                    };
                    cookieContainer.Add(cookie);
                }
                else if (header.Key.StartsWith(HttpHeaderReferer, StringComparison.InvariantCultureIgnoreCase))
                {
                    // https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Reference/Headers/Referer
                    if (Uri.TryCreate(header.Value, UriKind.Absolute, out var refererUri))
                    {
                        req.Headers.Referrer = refererUri;
                    }
                }
                else
                {
                    var headerKey = header.Key[..^HttpHeader_SteamTool.Length];
                    req.Headers.TryAddWithoutValidation(headerKey, (IEnumerable<string?>)header.Value);
                }
            }
        }
        req.Headers.UserAgent.TryParseAdd(context.Request.Headers.UserAgent);
        if (req.Content != null)
        {
            if (context.Request.ContentType != null)
            {
                if (MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var parsedValue))
                {
                    req.Content.Headers.ContentType = parsedValue;
                }
            }
            req.Content.Headers.ContentLength = context.Request.ContentLength;
        }
        SetRequest(c, req);
        var rsp = await c.SendAsync(req,
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted).ConfigureAwait(false);
        context.Response.StatusCode = unchecked((int)rsp.StatusCode);
        if (rsp.Content != null)
        {
            foreach (var header in rsp.Content.Headers)
            {
                StringValues svalues;
                if (header.Value == null)
                {
                    svalues = StringValues.Empty;
                }
                else if (header.Value is string?[] values)
                {
                    svalues = new(values);
                }
                else
                {
                    svalues = new StringValues(header.Value.ToArray());
                }
                context.Response.Headers.Append(header.Key, svalues);
            }
            using var body = await rsp.Content.ReadAsStreamAsync(context.RequestAborted);
            await body.CopyToAsync(context.Response.BodyWriter.AsStream(), context.RequestAborted);
        }
    }

    static void SetRequest(HttpClient c, HttpRequestMessage r)
    {
        r.Version = c.DefaultRequestVersion;
        r.VersionPolicy = c.DefaultVersionPolicy;
    }

    static Task HandleStatusCodeAsync(HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 返回未匹配信息
    /// </summary>
    static Task Handle404NotFoundAsync(HttpContext context)
        => HandleStatusCodeAsync(context, StatusCodes.Status404NotFound);
}