using AigioL.Common.Extensions.Http.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Extensions.Http.Models;

/// <summary>
/// 记录 <see cref="HttpRequestMessage"/> 的模型类
/// </summary>
public partial class HttpRequestMessageRecord
{
    public static readonly HttpRequestOptionsKey<Uri?> KeyRequestUri = new(nameof(HttpRequestMessage.RequestUri));
    public static readonly HttpRequestOptionsKey<CookieContainer> KeyCookieContainer = new(nameof(CookieContainer));
    public static readonly HttpRequestOptionsKey<IWebProxy?> KeyWebProxy = new(nameof(WebProxy));

    Uri? requestUri;
    string? requestUriHide;

    /// <inheritdoc cref="HttpRequestMessage.RequestUri"/>
    [JsonIgnore]
    public Uri? RequestUri
    {
        get => requestUri;
        set
        {
            requestUri = value;
            requestUriHide = null;
        }
    }

    /// <summary>
    /// 隐藏 Query 部分，脱敏数据
    /// </summary>
    [return: NotNullIfNotNull(nameof(requestUri))]
    public static string? GetRequestUriHideQuery(Uri? requestUri)
    {
        if (requestUri == null)
        {
            return null;
        }
        // Query 参数不记录日志，避免敏感信息泄露！！！
        var absolutePath = string.IsNullOrEmpty(requestUri.AbsolutePath) ? default : requestUri.AbsolutePath.AsSpan();
        absolutePath = absolutePath.TrimEnd('/');
        var requestUriHide = $"{requestUri.Scheme}://{requestUri.Host}{absolutePath}";
        return requestUriHide;
    }

    /// <summary>
    /// <see cref="HttpRequestMessage.RequestUri"/> 的隐藏 Query 部分，脱敏数据
    /// </summary>
    public string? RequestUriHideQuery
    {
        get
        {
            if (requestUriHide == null)
            {
                if (requestUri != null)
                {
                    return GetRequestUriHideQuery(requestUri);
                }
            }
            return requestUriHide;
        }
        set
        {
            requestUriHide = value;
            if (requestUri == null && !string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                requestUri = uri;
            }
        }
    }

    /// <inheritdoc cref="HttpRequestMessage.Method"/>
    public string? Method { get; set; }

    /// <inheritdoc cref="HttpRequestMessage.Version"/>
    public Version? Version { get; set; }

    /// <inheritdoc cref="HttpRequestMessage.VersionPolicy"/>
    public HttpVersionPolicy? VersionPolicy { get; set; }

    /// <inheritdoc cref="HttpContentHeaders.ContentType"/>
    [JsonConverter(typeof(MediaTypeHeaderValueToStringJsonConverter))]
    public MediaTypeHeaderValue? ContentType { get; set; }
}

partial class HttpRequestMessageRecord
{
    /// <summary>
    /// 获取请求的原始地址，请求地址 <see cref="HttpRequestMessage.RequestUri"/> 可能被重定向修改，必须通过 <see cref="HttpClientExtensions.UseDefaultSendAsync"/> 发送请求才能获取真实原始地址
    /// </summary>
    public static Uri? GetOriginalRequestUri(HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(KeyRequestUri, out var requestUri) && requestUri != null)
        {
            return requestUri;
        }
        else
        {
            return request.RequestUri;
        }
    }

    public virtual void SetRequestMessage(HttpRequestMessage request)
    {
        RequestUri = GetOriginalRequestUri(request);
        Method = request.Method.Method;
        Version = request.Version;
        VersionPolicy = request.VersionPolicy;

        ContentType = request.Content?.Headers.ContentType;
    }

    public override string ToString()
    {
        var str = JsonSerializer.Serialize(this, ExHttpJsonSerializerContext.Default.HttpRequestMessageRecord);
        return str;
    }
}
