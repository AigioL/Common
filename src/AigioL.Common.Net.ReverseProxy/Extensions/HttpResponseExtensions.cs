using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// <see cref="HttpResponse"/> 的扩展方法
/// </summary>
public static partial class HttpResponseExtensions
{
    /// <summary>
    /// 从 HTTP 响应的 <see cref="HttpResponse.ContentType"/> 中读取响应内容的 <see cref="Encoding"/>
    /// </summary>
    public static Encoding GetEncoding(this HttpResponse response)
    {
        var encoding = SysNetHttpContent.ReadBufferAsString(response.Body, response.ContentType);
        return encoding;
    }
}

file static class SysNetHttpContent
{
    internal static Encoding GetDefaultStringEncoding() => Encoding.UTF8;

    static ReadOnlySpan<byte> UTF8Preamble => [0xEF, 0xBB, 0xBF];

    static ReadOnlySpan<byte> UTF32Preamble => [0xFF, 0xFE, 0x00, 0x00];

    static ReadOnlySpan<byte> UnicodePreamble => [0xFF, 0xFE];

    static ReadOnlySpan<byte> BigEndianUnicodePreamble => [0xFE, 0xFF];

    const int MaxPreambleLength = 4;

    static Encoding GetEncoding(ReadOnlySpan<char> name)
    {
        return Encoding.GetEncoding(name.ToString());
    }

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Net.Http/src/System/Net/Http/HttpContent.cs#L113-L191
    /// </summary>
    internal static Encoding ReadBufferAsString(Stream stream, string? contentType)
    {
        if (stream.Length == 0)
        {
            return GetDefaultStringEncoding();
        }

        // We don't validate the Content-Encoding header: If the content was encoded, it's the caller's
        // responsibility to make sure to only call ReadAsString() on already decoded content. E.g. if the
        // Content-Encoding is 'gzip' the user should set HttpClientHandler.AutomaticDecompression to get a
        // decoded response stream.

        Encoding? encoding = null;

        ReadOnlySpan<char> charset = [];
        if (!string.IsNullOrEmpty(contentType) && MediaTypeHeaderValue.TryParse(contentType, out var contentType2))
        {
            charset = contentType2.Charset;
        }

        // If we do have encoding information in the 'Content-Type' header, use that information to convert
        // the content to a string.
        if (!charset.IsEmpty)
        {
            try
            {
                // Remove at most a single set of quotes.
                if (charset.Length > 2 &&
                    charset.StartsWith('\"') &&
                    charset.EndsWith('\"'))
                {
                    encoding = GetEncoding(charset[1..^1]);
                }
                else
                {
                    encoding = GetEncoding(charset);
                }

                // Byte-order-mark (BOM) characters may be present even if a charset was specified.
                return encoding;
            }
            catch /*(ArgumentException e)*/
            {
                //throw new InvalidOperationException(SR.net_http_content_invalid_charset, e);
            }
        }

        // If no content encoding is listed in the ContentType HTTP header, or no Content-Type header present,
        // then check for a BOM in the data to figure out the encoding.
        if (encoding == null)
        {
            Span<byte> firstBuffer = stackalloc byte[MaxPreambleLength];
            stream.Position = 0;
            var len = stream.Read(firstBuffer);
            stream.Position = 0;
            Debug.Assert(firstBuffer.Length >= 4 || firstBuffer.Length == stream.Length);
            if (len == MaxPreambleLength)
            {
                if (TryDetectEncoding(firstBuffer, out encoding))
                {
                    return encoding;
                }
            }
        }

        return GetDefaultStringEncoding();
    }

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Net.Http/src/System/Net/Http/HttpContent.cs#L735-L768
    /// </summary>
    static bool TryDetectEncoding(ReadOnlySpan<byte> data, [NotNullWhen(true)] out Encoding? encoding/*, out int preambleLength*/)
    {
        if (data.StartsWith(UTF8Preamble))
        {
            encoding = Encoding.UTF8;
            //preambleLength = UTF8Preamble.Length;
            return true;
        }

        if (data.StartsWith(UTF32Preamble))
        {
            encoding = Encoding.UTF32;
            //preambleLength = UTF32Preamble.Length;
            return true;
        }

        if (data.StartsWith(UnicodePreamble))
        {
            encoding = Encoding.Unicode;
            //preambleLength = UnicodePreamble.Length;
            return true;
        }

        if (data.StartsWith(BigEndianUnicodePreamble))
        {
            encoding = Encoding.BigEndianUnicode;
            //preambleLength = BigEndianUnicodePreamble.Length;
            return true;
        }

        encoding = null;
        //preambleLength = 0;
        return false;
    }
}