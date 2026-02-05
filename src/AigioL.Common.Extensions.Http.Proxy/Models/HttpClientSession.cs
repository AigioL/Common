using AigioL.Common.Extensions.Http.Models;
using System.Net;

namespace AigioL.Common.Extensions.Http.Proxy.Models;

/// <summary>
/// 带状态的 <see cref="global::System.Net.Http.HttpClient"/>，状态为 Cookies 与 代理信息，可使用由业务方管理的 HttpClient 或 IHttpClientFactory 工厂创建的实例，附加状态，必须使用 <see cref="HttpClientExtensions.UseDefaultSendAsync"/>
/// </summary>
public sealed partial class HttpClientSession
{
    /// <summary>
    /// 业务方管理的 HttpClient，在 ASP.NET Core 中必须使用 IHttpClientFactory 避免 Socket 耗尽
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>
    /// 使用 IHttpClientFactory 时，附加的 Cookies 容器
    /// </summary>
    public CookieContainer? CookieContainer { get; init; }

    /// <summary>
    /// 使用 IHttpClientFactory 时，附加的代理信息
    /// </summary>
    public IWebProxy? WebProxy { get; set; }

    public void SetNoProxy() => WebProxy = HttpNoProxy.Instance;

    /// <summary>
    /// 当前是否为业务方管理的 HttpClient
    /// </summary>
    public bool IsIncomingHttpClient { get; init; }

    /// <summary>
    /// 将附加的状态设置到请求消息中
    /// </summary>
    /// <param name="req"></param>
    public void SetOptions(HttpRequestMessage req)
    {
        if (IsIncomingHttpClient)
        {
            return;
        }

        if (CookieContainer != null)
        {
            req.Options.Set(HttpRequestMessageRecord.KeyCookieContainer, CookieContainer);
        }

        if (WebProxy != null)
        {
            req.Options.Set(HttpRequestMessageRecord.KeyWebProxy, WebProxy);
        }

        // UseDefaultSendAsync 会处理这些选项
    }

    public HttpClient GetHttpClient(IHttpClientFactory httpClientFactory, string name)
    {
        if (IsIncomingHttpClient)
        {
            ArgumentNullException.ThrowIfNull(HttpClient);
            return HttpClient;
        }

        return httpClientFactory.CreateClient(name);
    }

    public static implicit operator HttpClientSession(HttpClient httpClient) => new() { HttpClient = httpClient, IsIncomingHttpClient = true, };

    public static implicit operator HttpClientSession(CookieContainer cookieContainer) => new() { CookieContainer = cookieContainer };

    public static implicit operator HttpClientSession((CookieContainer cookieContainer, IWebProxy? webProxy) t) => new() { CookieContainer = t.cookieContainer, WebProxy = t.webProxy };
}
