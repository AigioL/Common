using Microsoft.IO;
using System.Buffers;
using System.Net.Http.Headers;
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

    public static (HashAlgorithmTypeName? hashAlgorithmTypeName, ReadOnlyMemory<char> authorizationValue, ReadOnlyMemory<char> authorizationLeft) GetAuthorizationValue(ReadOnlyMemory<char> authorization)
    {
        if (authorization.IsEmpty)
        {
            return default;
        }

        var index = authorization.Span.IndexOf(' ');
        if (index >= 0)
        {
            var authorizationLeft = authorization[..index].Trim();
            index = authorizationLeft.Span.IndexOf(DefaultSignatureAlgorithmPrefix, StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
            {
                var chars = authorizationLeft.Span[(index + DefaultSignatureAlgorithmPrefix.Length)..];
                chars = chars.Trim();
                if (Enum.TryParse<HashAlgorithmTypeName>(chars, true, out var hashAlgorithmTypeName))
                {
                    var authorizationValue = authorization[index..].Trim();
                    return (hashAlgorithmTypeName, authorizationValue, authorizationLeft);
                }
            }
        }

        return default;
    }
}

static partial class ApiSignatureHelper // Const
{
    public const string DefaultSignatureAlgorithmPrefix = "HMAC-";

    /// <summary>
    /// 默认的签名算法，使用 <see cref="HMACSHA256"/> 哈希算法进行签名计算
    /// </summary>
    public const string DefaultSignatureAlgorithm = $"{DefaultSignatureAlgorithmPrefix}SHA256";

    const string accessKeyPrefix = "AK=";
    const string signaturePrefix = "Signature=";
    const string signedHeadersPrefix = "SignedHeaders=";

    public const string SdkDateHeaderName = "sdk-date";
    const string HostHeaderName = "host";

    static ReadOnlySpan<byte> AccessKeyU8() => "AK="u8;
    static ReadOnlySpan<byte> SignatureU8() => "Signature="u8;
    static ReadOnlySpan<byte> SignedHeadersU8() => "SignedHeaders="u8;
}

static partial class ApiSignatureHelper // Private
{
    static readonly RecyclableMemoryStreamManager m = new();

    /// <summary>
    /// 向缓冲区中写入规范化请求的哈希值
    /// </summary>
    /// <param name="s"></param>
    /// <param name="method">HTTP 请求方法，全大写字母</param>
    /// <param name="destinationChars"></param>
    /// <param name="hashAlgorithmTypeName"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="canonicalUri">规范化 URI</param>
    /// <param name="canonicalQueryString">规范化查询字符串</param>
    /// <param name="headers">请求头</param>
    /// <param name="hashedRequestPayload">RequestBody 的哈希值</param>
    /// <param name="key"></param>
    /// <param name="destination"></param>
    public static async ValueTask WriteHashedCanonicalRequest<THeaderValue>(
        string method,
        string? canonicalUri,
        string? canonicalQueryString,
        IEnumerable<KeyValuePair<string, THeaderValue>> headers,
        ReadOnlyMemory<char> hashedRequestPayload,
        Memory<byte> destination,
        Memory<char> destinationChars = default,
        HashAlgorithmTypeName hashAlgorithmTypeName = HashAlgorithmTypeName.SHA256,
        CancellationToken cancellationToken = default)
        where THeaderValue : notnull
    {
        using var s = m.GetStream();

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

        s.Write(hashedRequestPayload.Span);
        s.Write("\n"u8);

        s.Position = 0;

        await hashAlgorithmTypeName.HashDataAsync(
            s, destination, destinationChars,
            true, cancellationToken);
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
    public static async ValueTask WriteSignatureAsync(
        ReadOnlyMemory<byte> appSecret,
        ReadOnlyMemory<char> signatureAlgorithm,
        ReadOnlyMemory<char> hashedCanonicalRequest,
        Memory<byte> destination,
        Memory<char> destinationChars = default,
        HashAlgorithmTypeName hashAlgorithmTypeName = HashAlgorithmTypeName.SHA256,
        CancellationToken cancellationToken = default)
    {
        using var s = m.GetStream(); // StringToSign

        s.Write(signatureAlgorithm.Span);
        s.Write("\n"u8);
        s.Write(hashedCanonicalRequest.Span);

        // Signature = HexEncode(SignatureMethod(Secret, StringToSign))
        await hashAlgorithmTypeName.HMACHashDataAsync(
            appSecret, s, destination,
            destinationChars, true, cancellationToken);
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

    /// <summary>
    /// 计算签名并设置 Authorization 请求头
    /// </summary>
    public static async ValueTask SetAuthorizationAsync(
        string appAccessKey,
        ReadOnlyMemory<byte> appSecret,
        HashAlgorithmTypeName hashAlgorithmTypeName,
        HttpRequestMessage request,
        IEnumerable<string>? signedHeaders = null,
        DateTime? sdkDate = null,
        CancellationToken cancellationToken = default)
    {
        signedHeaders ??= new HashSet<string>();

        sdkDate ??= DateTime.UtcNow;
        request.Headers.TryAddWithoutValidation(SdkDateHeaderName,
            sdkDate.Value.ToString("o") // https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/standard-date-and-time-format-strings#the-round-trip-o-o-format-specifier
        );
        if (!signedHeaders.Contains(SdkDateHeaderName))
        {
            signedHeaders = signedHeaders.Append(SdkDateHeaderName);
        }

        if (string.IsNullOrWhiteSpace(request.Headers.Host))
        {
            request.Headers.Host = request.RequestUri?.Host;
        }
        if (!signedHeaders.Contains(HostHeaderName))
        {
            signedHeaders = signedHeaders.Append(HostHeaderName);
        }

        var requestBody = request.Content == null ? null : (await request.Content.ReadAsStreamAsync(cancellationToken));

        var hashSizeInBytes = hashAlgorithmTypeName.GetHMACHashSizeInBytes();

        var signatureBytes = ArrayPool<byte>.Shared.Rent(hashSizeInBytes);
        var signatureChars = ArrayPool<char>.Shared.Rent(hashSizeInBytes * 2);

        try
        {
            // 1. 将 RequestBody 进行哈希计算，得到 HashedRequestPayload
            await hashAlgorithmTypeName.HashDataAsync(
                requestBody, signatureBytes, signatureChars,
                true, cancellationToken);

#if DEBUG
            var debugView_HashedRequestPayload = new string(signatureChars, 0, hashSizeInBytes * 2);
#endif

            // 2. 构造规范化请求并进行哈希计算，得到 HashedCanonicalRequest
            var method = request.Method.Method;
            var canonicalUri = request.RequestUri?.AbsolutePath;
            var canonicalQueryString = request.RequestUri?.Query;
            var headers = request.Headers.Where(x => signedHeaders.Any(y => y.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase)));

            await WriteHashedCanonicalRequest(
                method, canonicalUri, canonicalQueryString,
                headers, signatureChars.AsMemory(0, hashSizeInBytes * 2), signatureBytes,
                signatureChars, hashAlgorithmTypeName,
                cancellationToken);

#if DEBUG
            var debugView_HashedCanonicalRequest = new string(signatureChars, 0, hashSizeInBytes * 2);
#endif

            var signatureAlgorithm = $"{DefaultSignatureAlgorithmPrefix}{hashAlgorithmTypeName}";

            // 3. 构造待签名字符串，通过 HMAC 计算签名
            await WriteSignatureAsync(
                appSecret, signatureAlgorithm.AsMemory(), signatureChars.AsMemory(0, hashSizeInBytes * 2),
                signatureBytes, signatureChars, hashAlgorithmTypeName,
                cancellationToken);

#if DEBUG
            var debugView_Signature = new string(signatureChars, 0, hashSizeInBytes * 2);
#endif

            var authorization = GetAuthorization(appAccessKey, signedHeaders, signatureChars.AsSpan(0, hashSizeInBytes * 2), signatureAlgorithm);
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(signatureBytes);
            ArrayPool<char>.Shared.Return(signatureChars);
        }
    }
}