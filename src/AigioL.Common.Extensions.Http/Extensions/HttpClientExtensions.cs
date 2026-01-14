#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

public static partial class HttpClientExtensions
{
    static void UseDefault(
        HttpClient httpClient,
        HttpRequestMessage request)
    {
#if NETSTANDARD || NETFRAMEWORK
        //request.Version = DefaultRequestVersion;
#else
        request.Version = httpClient.DefaultRequestVersion;
        request.VersionPolicy = httpClient.DefaultVersionPolicy;
#endif
    }

    /// <summary>
    /// 使用 <see cref="HttpClient"/> 的默认设置发送请求
    /// </summary>
    public static Task<HttpResponseMessage> UseDefaultSendAsync(
        this HttpClient httpClient,
        HttpRequestMessage request,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        CancellationToken cancellationToken = default)
    {
        UseDefault(httpClient, request);
        return httpClient.SendAsync(request, completionOption, cancellationToken);
    }
}
