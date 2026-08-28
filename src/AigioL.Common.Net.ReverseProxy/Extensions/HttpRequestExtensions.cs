using Microsoft.AspNetCore.Http.Extensions;
using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// <see cref="HttpRequest"/> 的扩展方法
/// </summary>
public static partial class HttpRequestExtensions
{
    /// <summary>
    /// 将请求的 <see cref="UriHelper.GetDisplayUrl(HttpRequest)"/> 字符串以 UTF-8 编码写入响应流中
    /// <para>避免分配新的字符串实例</para>
    /// </summary>
    public static void WriteDisplayUrl(this HttpRequest request)
    {
        var response = request.HttpContext.Response;

        // https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Extensions/src/UriHelper.cs#L206
        response.BodyWriter.Write(request.Scheme);
        response.BodyWriter.Write("://"u8);
        response.BodyWriter.Write(request.Host.Value);
        response.BodyWriter.Write(request.PathBase.Value);
        response.BodyWriter.Write(request.Path.Value);
        response.BodyWriter.Write(request.QueryString.Value);
    }

    /// <summary>
    /// 获取请求的 <see cref="UriHelper.GetDisplayUrl(HttpRequest)"/> 的字符串长度
    /// </summary>
    public static int GetDisplayUrlLength(this HttpRequest request) => request.Scheme.Length
        + 3
        + (string.IsNullOrEmpty(request.Host.Value) ? 0 : request.Host.Value.Length)
        + (string.IsNullOrEmpty(request.PathBase.Value) ? 0 : request.PathBase.Value.Length)
        + (string.IsNullOrEmpty(request.Path.Value) ? 0 : request.Path.Value.Length)
        + (string.IsNullOrEmpty(request.QueryString.Value) ? 0 : request.QueryString.Value.Length);

    /// <summary>
    /// 将请求的 <see cref="UriHelper.GetDisplayUrl(HttpRequest)"/> 字符串以 UTF-8 编码写入 <see cref="Span{T}"/> 中
    /// <para>避免分配新的字符串实例</para>
    /// </summary>
    public static void WriteDisplayUrl(this HttpRequest request, Span<char> buffers)
    {
        var written = 0;

        request.Scheme.AsSpan().CopyTo(buffers[written..]);
        written += request.Scheme.Length;

        "://".AsSpan().CopyTo(buffers[written..]);
        written += 3;

        if (!string.IsNullOrEmpty(request.Host.Value))
        {
            request.Host.Value.AsSpan().CopyTo(buffers[written..]);
            written += request.Host.Value.Length;
        }

        if (!string.IsNullOrEmpty(request.PathBase.Value))
        {
            request.PathBase.Value.AsSpan().CopyTo(buffers[written..]);
            written += request.PathBase.Value.Length;
        }

        if (!string.IsNullOrEmpty(request.Path.Value))
        {
            request.Path.Value.AsSpan().CopyTo(buffers[written..]);
            written += request.Path.Value.Length;
        }

        if (!string.IsNullOrEmpty(request.QueryString.Value))
        {
            request.QueryString.Value.AsSpan().CopyTo(buffers[written..]);
            //written += request.QueryString.Value.Length;
        }
    }

    /// <summary>
    /// 获取请求的 <see cref="UriHelper.GetDisplayUrl(HttpRequest)"/> 的 HTTPS 协议的字符串，通常用于 HTTP 重定向到 HTTPS
    /// </summary>
    public static string GetDisplayUrlHttps(this HttpRequest request)
    {
        return string.Concat([Uri.UriSchemeHttps, "://", request.Host.Value, request.PathBase.Value, request.Path.Value, request.QueryString.Value]);
    }

    /// <summary>
    /// 获取请求的原始 URL 的字符串长度
    /// <para>
    /// 原始 URL 定义为域信息后面的 URL 的一部分，在 URL 字符串 http://www.contoso.com/articles/recent.aspx 中，原始 URL 为 /articles/recent.aspx，原始 URL 包括查询字符串（如果存在）
    /// </para>
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.web.httprequest.rawurl</para>
    /// </summary>
    public static int GetRawUrlLength(this HttpRequest request) =>
        (string.IsNullOrEmpty(request.PathBase.Value) ? 0 : request.PathBase.Value.Length)
        + (string.IsNullOrEmpty(request.Path.Value) ? 0 : request.Path.Value.Length)
        + (string.IsNullOrEmpty(request.QueryString.Value) ? 0 : request.QueryString.Value.Length);

    /// <summary>
    /// 将请求的原始 URL 的字符串写入 <see cref="Span{T}"/> 中
    /// <para>
    /// 原始 URL 定义为域信息后面的 URL 的一部分，在 URL 字符串 http://www.contoso.com/articles/recent.aspx 中，原始 URL 为 /articles/recent.aspx，原始 URL 包括查询字符串（如果存在）
    /// </para>
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.web.httprequest.rawurl</para>
    /// <para>避免分配新的字符串实例</para>
    /// </summary>
    public static void WriteRawUrl(this HttpRequest request, Span<char> buffers)
    {
        var written = 0;

        if (!string.IsNullOrEmpty(request.PathBase.Value))
        {
            request.PathBase.Value.AsSpan().CopyTo(buffers[written..]);
            written += request.PathBase.Value.Length;
        }

        if (!string.IsNullOrEmpty(request.Path.Value))
        {
            request.Path.Value.AsSpan().CopyTo(buffers[written..]);
            written += request.Path.Value.Length;
        }

        if (!string.IsNullOrEmpty(request.QueryString.Value))
        {
            request.QueryString.Value.AsSpan().CopyTo(buffers[written..]);
            written += request.QueryString.Value.Length;
        }
    }

    static ArgumentOutOfRangeException GetUnknownHttpVersionException(string? actualValue, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null) => new(
$"""
Version doesn't map to a known HTTP protocol. (Parameter '{paramName}')
Actual value was {actualValue}.
""");

    /// <summary>
    /// 获取请求的 HTTP 协议版本
    /// </summary>
    public static Version GetHttpVersion(this HttpRequest request) => GetHttpVersion(request.Protocol);

    static Version GetHttpVersion(string requestProtocol)
    {
        // 参考 https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Abstractions/src/HttpProtocol.cs
        if (!string.IsNullOrWhiteSpace(requestProtocol) && requestProtocol.Length >= 6)
        {
            if (char.ToUpperInvariant(requestProtocol[0]) == 'H' &&
                char.ToUpperInvariant(requestProtocol[1]) == 'T' &&
                char.ToUpperInvariant(requestProtocol[2]) == 'T' &&
                char.ToUpperInvariant(requestProtocol[3]) == 'P' &&
                char.ToUpperInvariant(requestProtocol[4]) == '/')
            {
                if (requestProtocol.Length == 6)
                {
                    switch (requestProtocol[5])
                    {
                        case '1':
                            return HttpVersion.Version10;
                        case '2':
                            return HttpVersion.Version20;
                        case '3':
                            return HttpVersion.Version30;
                    }
                }

                if (requestProtocol.Length == 8 && requestProtocol[6] == '.')
                {
                    switch (requestProtocol[5])
                    {
                        case '1':
                            {
                                switch (requestProtocol[7])
                                {
                                    case '0':
                                        return HttpVersion.Version10;
                                    case '1':
                                        return HttpVersion.Version11;
                                }
                            }
                            break;
                        case '2':
                            {
                                switch (requestProtocol[7])
                                {
                                    case '0':
                                        return HttpVersion.Version20;
                                }
                            }
                            break;
                        case '3':
                            {
                                switch (requestProtocol[7])
                                {
                                    case '0':
                                        return HttpVersion.Version30;
                                }
                            }
                            break;
                    }
                }

                var requestProtocolVersion = requestProtocol.AsSpan(5);
                if (requestProtocolVersion.Contains('.'))
                {
                    if (Version.TryParse(requestProtocolVersion, out var v))
                    {
                        return v;
                    }
                }
                else
                {
                    if (int.TryParse(requestProtocolVersion, out var i))
                    {
                        return new Version(i, 0);
                    }
                }
            }
        }
        throw GetUnknownHttpVersionException(requestProtocol);
    }
}
