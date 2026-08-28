using Microsoft.AspNetCore.Http.Extensions;
using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

public static partial class HttpRequestExtensions
{
    /// <summary>
    /// 将请求的 <see cref="UriHelper.GetDisplayUrl(HttpRequest)"/> 字符串以 UTF-8 编码写入响应流中
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

    public static int GetDisplayUrlLength(this HttpRequest request) => request.Scheme.Length
        + 3
        + (string.IsNullOrEmpty(request.Host.Value) ? 0 : request.Host.Value.Length)
        + (string.IsNullOrEmpty(request.PathBase.Value) ? 0 : request.PathBase.Value.Length)
        + (string.IsNullOrEmpty(request.Path.Value) ? 0 : request.Path.Value.Length)
        + (string.IsNullOrEmpty(request.QueryString.Value) ? 0 : request.QueryString.Value.Length);

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
            written += request.QueryString.Value.Length;
        }
    }

    public static string GetDisplayUrlHttps(this HttpRequest request)
    {
        return string.Concat([Uri.UriSchemeHttps, "://", request.Host.Value, request.PathBase.Value, request.Path.Value, request.QueryString.Value]);
    }

    public static int GetRawUrlLength(this HttpRequest request) =>
        (string.IsNullOrEmpty(request.PathBase.Value) ? 0 : request.PathBase.Value.Length)
        + (string.IsNullOrEmpty(request.Path.Value) ? 0 : request.Path.Value.Length)
        + (string.IsNullOrEmpty(request.QueryString.Value) ? 0 : request.QueryString.Value.Length);

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
