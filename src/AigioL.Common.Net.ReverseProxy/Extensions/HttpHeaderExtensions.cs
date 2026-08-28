using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// <see cref="IHeaderDictionary"/> 的扩展方法
/// </summary>
public static partial class HttpHeaderExtensions
{
    /// <summary>
    /// 获取 <see cref="IHeaderDictionary"/> 中的 <see cref="HeaderNames.ContentEncoding"/> 头部值，并按优先级排序
    /// </summary>
    public static IEnumerable<HttpContentEncodingSingle> GetContentEncoding(this IHeaderDictionary h)
    {
        var v = h.ContentEncoding;
        if (!StringValues.IsNullOrEmpty(v))
        {
            if (v.Count == 1)
            {
                return [GetHttpContentEncodingSingle(v[0])];
            }
            else
            {
                return GetContentEncodingCore(v).OrderByDescending(static x => x);
            }
        }
        return [];
    }

    static HttpContentEncodingSingle GetHttpContentEncodingSingle(ReadOnlySpan<char> v)
    {
        if ("zstd".Equals(v, StringComparison.InvariantCultureIgnoreCase))
        {
            return HttpContentEncodingSingle.Zstd;
        }
        else if ("br".Equals(v, StringComparison.InvariantCultureIgnoreCase))
        {
            return HttpContentEncodingSingle.Br;
        }
        else if ("gzip".Equals(v, StringComparison.InvariantCultureIgnoreCase))
        {
            return HttpContentEncodingSingle.Gzip;
        }
        else if ("compress".Equals(v, StringComparison.InvariantCultureIgnoreCase))
        {
            return HttpContentEncodingSingle.Compress;
        }
        else if ("deflate".Equals(v, StringComparison.InvariantCultureIgnoreCase))
        {
            return HttpContentEncodingSingle.Deflate;
        }

        return HttpContentEncodingSingle.Null;
    }

    static IEnumerable<HttpContentEncodingSingle> GetContentEncodingCore(StringValues v)
    {
        for (int i = 0; i < v.Count; i++)
        {
            var it = v[i];
            if (string.Equals("zstd", it, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return HttpContentEncodingSingle.Zstd;
            }
            else if (string.Equals("br", it, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return HttpContentEncodingSingle.Br;
            }
            else if (string.Equals("gzip", it, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return HttpContentEncodingSingle.Gzip;
            }
            else if (string.Equals("compress", it, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return HttpContentEncodingSingle.Compress;
            }
            else if (string.Equals("deflate", it, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return HttpContentEncodingSingle.Deflate;
            }
        }
    }

    public static Stream? GetCompressionStream(this HttpContentEncodingSingle contentEncoding, Stream stream, CompressionMode mode, bool leaveOpen) => contentEncoding switch
    {
        HttpContentEncodingSingle.Gzip => new GZipStream(stream, mode, leaveOpen),
        HttpContentEncodingSingle.Compress => throw new NotSupportedException("不支持的响应头编码，Content-Encoding: compress"),
        HttpContentEncodingSingle.Deflate => new DeflateStream(stream, mode, leaveOpen),
        HttpContentEncodingSingle.Br => new BrotliStream(stream, mode, leaveOpen),
        HttpContentEncodingSingle.Zstd => new ZstandardStream(stream, mode, leaveOpen),
        _ => null,
    };
}
