using AigioL.Common.Extensions.Http.Models;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

public static partial class HttpClientExtensions
{
    static void UseDefault(
        HttpClient httpClient,
        HttpRequestMessage request,
        out bool useCookies)
    {
        useCookies = false;
#if NETSTANDARD || NETFRAMEWORK
        //request.Version = DefaultRequestVersion;
#else
        request.Version = httpClient.DefaultRequestVersion;
        request.VersionPolicy = httpClient.DefaultVersionPolicy;
#endif
        // 将原始请求地址存储在请求选项中，在 30x 状态码重定向后可以使用次取出原始地址
        if (request.RequestUri != null)
        {
            request.Options.Set(HttpRequestMessageRecord.KeyRequestUri, request.RequestUri);
            if (request.Options.TryGetValue(HttpRequestMessageRecord.KeyCookieContainer, out var cookieContainer) && cookieContainer != null)
            {
                useCookies = true;
                // https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Reference/Headers/Cookie
                var cookiesFromContainer = cookieContainer.GetCookieHeader(request.RequestUri);
                request.Headers.TryAddWithoutValidation("Cookie", cookiesFromContainer);
            }
        }
    }

    /// <summary>
    /// 使用 <see cref="HttpClient"/> 的默认设置发送请求
    /// </summary>
    public static async Task<HttpResponseMessage> UseDefaultSendAsync(
        this HttpClient httpClient,
        HttpRequestMessage request,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        CancellationToken cancellationToken = default)
    {
        UseDefault(httpClient, request, out var useCookies);
        var rsp = await httpClient.SendAsync(request, completionOption, cancellationToken);
        if (useCookies)
        {
            if (request.Options.TryGetValue(HttpRequestMessageRecord.KeyCookieContainer, out var cookieContainer) && cookieContainer != null)
            {
                rsp.ProcessReceivedCookies(cookieContainer);
            }
        }
        return rsp;
    }
}
