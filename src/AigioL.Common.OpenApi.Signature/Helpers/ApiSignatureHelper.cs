using Microsoft.IO;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.OpenApi.Signature.Helpers;

/// <summary>
/// API 签名帮助类
/// <para>参考 阿里云 SDK V3 版本请求体 &amp; 签名机制实现 https://help.aliyun.com/zh/sdk/product-overview/v3-request-structure-and-signature</para>
/// <para>华为云 API 签名认证机制 https://support.huaweicloud.com/devg-apisign/api-sign-algorithm-001.html</para>
/// </summary>
public static partial class ApiSignatureHelper
{
    /// <summary>
    /// 从 Authorization 请求头中解析出访问密钥、已签名的请求头列表和签名值
    /// </summary>
    public static (ReadOnlyMemory<char> accessKey, IReadOnlyList<ReadOnlyMemory<char>>? signedHeaders, ReadOnlyMemory<char> signature) DeconstructAuthorization(
        ReadOnlyMemory<char> authorization)
    {
        if (authorization.IsEmpty)
        {
            return default;
        }

        ReadOnlyMemory<char> accessKey = default;
        IReadOnlyList<ReadOnlyMemory<char>>? signedHeaders = null;
        ReadOnlyMemory<char> signature = default;

        var split = authorization.Span.Split(',');
        while (split.MoveNext())
        {
            var it = authorization[split.Current].Trim();
            if (it.Span.StartsWith(accessKeyPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                accessKey = it[accessKeyPrefix.Length..].Trim();
            }
            else if (it.Span.StartsWith(signaturePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                signature = it[signaturePrefix.Length..].Trim();
            }
            else if (it.Span.StartsWith(signedHeadersPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                var signedHeadersSplit = it.Span[signedHeadersPrefix.Length..].Split(';');
                List<ReadOnlyMemory<char>> signedHeadersList = new();
                while (signedHeadersSplit.MoveNext())
                {
                    var signedHeader = it[signedHeadersSplit.Current].Trim();
                    if (!signedHeader.IsEmpty)
                    {
                        signedHeadersList.Add(signedHeader);
                    }
                }
                if (signedHeadersList.Count > 0)
                {
                    signedHeaders = signedHeadersList;
                }
            }
        }

        return (accessKey, signedHeaders, signature);
    }
}

static partial class ApiSignatureHelper // Const
{
    /// <summary>
    /// 默认的签名算法，使用 <see cref="HMACSHA256"/> 哈希算法进行签名计算
    /// </summary>
    public const string DefaultSignatureAlgorithm = "HMAC-SHA256";

    const string accessKeyPrefix = "AK=";
    const string signaturePrefix = "Signature=";
    const string signedHeadersPrefix = "SignedHeaders=";

    static ReadOnlySpan<byte> AccessKeyU8() => "AK="u8;
    static ReadOnlySpan<byte> SignatureU8() => "Signature="u8;
    static ReadOnlySpan<byte> SignedHeadersU8() => "SignedHeaders="u8;
}

static partial class ApiSignatureHelper // Private
{
    static readonly RecyclableMemoryStreamManager m = new();

    /// <summary>
    /// 将请求正文通过 <see cref="HMACSHA256"/> 哈希算法进行哈希计算，并将结果写入 buffer 中，如果传递了 chars 缓冲区，则将哈希结果转换为小写十六进制字符串并写入 chars 中
    /// </summary>
    /// <param name="hmacHashkey">HMAC-SHA256 哈希算法的密钥</param>
    /// <param name="requestBody">请求正文流</param>
    /// <param name="buffer">用于存储哈希结果的缓冲区，长度应大于或等于 <see cref="HMACSHA256.HashSizeInBytes"/></param>
    /// <param name="chars">可选的字符缓冲区，用于存储哈希结果的十六进制表示，长度应大于或等于 <see cref="HMACSHA256.HashSizeInBytes"/> * 2</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    static async ValueTask WriteHashedRequestPayloadAsync(
        ReadOnlyMemory<byte> hmacHashkey,
        Stream requestBody,
        Memory<byte> buffer,
        Memory<char> chars = default,
        CancellationToken cancellationToken = default)
    {
        var len = await HMACSHA256.HashDataAsync(hmacHashkey, requestBody, buffer, cancellationToken);
        buffer = buffer[..len];
        if (!chars.IsEmpty && chars.Length >= HMACSHA256.HashSizeInBytes * 2)
        {
            Convert.TryToHexStringLower(buffer.Span, chars.Span, out var _);
        }
    }

    /// <summary>
    /// 向流中写入规范化请求
    /// </summary>
    /// <param name="s"></param>
    /// <param name="method">HTTP 请求方法，全大写字母</param>
    /// <param name="canonicalUri">规范化 URI</param>
    /// <param name="canonicalQueryString">规范化查询字符串</param>
    /// <param name="headers">请求头</param>
    /// <param name="hashedRequestPayload">RequestBody 的哈希值</param>
    static void WriteCanonicalRequest<THeaderValue>(
        RecyclableMemoryStream s,
        string method,
        string canonicalUri,
        string canonicalQueryString,
        IEnumerable<KeyValuePair<string, THeaderValue>> headers,
        ReadOnlySpan<char> hashedRequestPayload)
        where THeaderValue : notnull
    {
        s.Write(method);
        s.Write("\n"u8);

        s.Write(canonicalUri);
        s.Write("\n"u8);

        s.Write(canonicalQueryString);
        s.Write("\n"u8);

        WriteCanonicalHeaders(s, headers);
        s.Write("\n"u8);

        WriteSignedHeaders(s, headers.Select(static x => x.Key));
        s.Write("\n"u8);

        s.Write(hashedRequestPayload);
        s.Write("\n"u8);
    }

    /// <summary>
    /// 向流中写入规范化请求头
    /// </summary>
    /// <param name="s"></param>
    /// <param name="headers">请求头</param>
    static void WriteCanonicalHeaders<THeaderValue>(
        RecyclableMemoryStream s,
        IEnumerable<KeyValuePair<string, THeaderValue>> headers)
        where THeaderValue : notnull
    {
        foreach (var header in headers)
        {
            s.WriteToLowerInvariant(header.Key);
            s.Write(":"u8);

            //if (header.Value is StringValues headerValueSV)
            //{

            //}

            s.Write(header.Value?.ToString());

            s.Write("\n"u8);
        }
    }

    /// <summary>
    /// 向流中写入已签名的请求头列表
    /// </summary>
    static void WriteSignedHeaders(
        RecyclableMemoryStream s,
        IEnumerable<string> headerNames)
    {
        ReadOnlySpan<byte> separator = ";"u8;
        int i = 0;
        foreach (var headerName in headerNames)
        {
            if (i != 0)
            {
                s.Write(separator);
            }
            s.WriteToLowerInvariant(headerName);
            i++;
        }
    }

    /// <summary>
    /// 写入构造待签名字符串
    /// </summary>
    static void WriteStringToSign(
        RecyclableMemoryStream s,
        string signatureAlgorithm,
        ReadOnlySpan<char> hashedCanonicalRequest)
    {
        s.Write(signatureAlgorithm);
        s.Write("\n"u8);
        s.Write(hashedCanonicalRequest);
    }

    /// <summary>
    /// 获取 Authorization 请求头
    /// </summary>
    static string GetAuthorization(
        string accessKey,
        IEnumerable<string> signedHeaders,
        ReadOnlySpan<char> signature,
        string signatureAlgorithm = DefaultSignatureAlgorithm)
    {
        using var s = m.GetStream();

        s.Write(signatureAlgorithm);
        s.Write(" "u8);

        s.Write(AccessKeyU8());
        s.Write(accessKey);
        s.Write(","u8);
        s.Write(SignedHeadersU8());
        WriteSignedHeaders(s, signedHeaders);
        s.Write(","u8);
        s.Write(SignatureU8());
        s.Write(signature);

        var str = Encoding.UTF8.GetString(s.GetReadOnlySequence());
        return str;
    }
}