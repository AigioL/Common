using AigioL.Common.Primitives.Columns;
using AigioL.Common.SmsSender.Models;
using AigioL.Common.SmsSender.Models.Abstractions;
using AigioL.Common.SmsSender.Models.Channels.HuaweiCloud;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Buffers;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using SmsOptions = AigioL.Common.SmsSender.Models.Channels.HuaweiCloud.SmsHuaweiCloudOptions;


namespace AigioL.Common.SmsSender.Services.Implementation.SmsSender.Channels.HuaweiCloud;

/// <summary>
/// 短信服务提供商 - 华为云
/// </summary>
public partial class SmsSenderProvider : SmsSenderBase, ISmsSender
{
    /// <summary>
    /// 华为云的名称
    /// </summary>
    public const string Name = nameof(HuaweiCloud);

    /// <inheritdoc/>
    public override string Channel => Name;

    /// <inheritdoc/>
    public override bool SupportCheck => false;

    readonly HttpClient httpClient;
    readonly SmsOptions options;
    readonly ILogger logger;

    /// <summary>
    /// 初始化 <see cref="SmsSenderProvider"/> 类的实例，设置所需的日志记录器、配置选项和 HttpClient
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="httpClient"></param>
    /// <exception cref="ArgumentException"></exception>
    public SmsSenderProvider(ILogger<SmsSenderProvider> logger, SmsOptions? options, HttpClient httpClient)
    {
        this.logger = logger;
        if (!options.HasValue()) throw new ArgumentException(null, nameof(options));
        this.options = options;
        this.httpClient = httpClient;
    }

    #region 常量

    const string BasicDateFormat = "yyyyMMddTHHmmssZ";
    const string Algorithm = "SDK-HMAC-SHA256";
    const string HeaderXDate = "X-Sdk-Date";
    const string HeaderHost = "host";
    const string HeaderAuthorization = "Authorization";
    const string HeaderContentSha256 = "X-Sdk-Content-Sha256";

    #endregion

    #region helpers

    static async Task<HttpRequestMessage> Sign(string appKey, string appSecret, string apiAddress, FormUrlEncodedContent bodyContent, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(HttpMethod.Post, apiAddress)
        {
            Content = bodyContent,
        };

        var time = request.Headers.TryGetValues(HeaderXDate, out var xdate) ? xdate.FirstOrDefault() : null;
        DateTime xdateTime;
        if (time == null)
        {
            xdateTime = DateTime.Now;
            request.Headers.TryAddWithoutValidation(HeaderXDate, xdateTime.ToUniversalTime().ToString(BasicDateFormat));
        }
        else
        {
            xdateTime = DateTime.ParseExact(time, BasicDateFormat, CultureInfo.CurrentCulture);
        }
        request.Headers.TryAddWithoutValidation(HeaderHost, request.RequestUri!.Host);
        var signedHeaders = SignedHeaders(request.Headers);
        using var canonicalRequest = await CanonicalRequest(request, signedHeaders, bodyContent, cancellationToken);
        using var stringToSign = StringToSign(canonicalRequest, xdateTime);
        var signingKey = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(appSecret.Length));
        string signature;
        try
        {
            Encoding.UTF8.TryGetBytes(appSecret, signingKey, out var bytesWritten);
            signature = SignStringToSign(stringToSign, signingKey.AsSpan(0, bytesWritten));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(signingKey);
        }
        var authValue = AuthHeaderValue(signature, appKey, signedHeaders);
        request.Headers.TryAddWithoutValidation(HeaderAuthorization, authValue);
        request.Headers.Remove(HeaderHost);
        return request;
    }

    static IList<string> SignedHeaders(HttpHeaders headers)
    {
        SortedList<string, string> a = new(StringComparer.Ordinal);
        foreach (string key in headers.Select(x => x.Key))
        {
            string keyLower = key.ToLowerInvariant();
            if (keyLower != "content-type")
            {
                a.Add(keyLower, keyLower);
            }
        }
        return a.Keys;
    }

    static readonly RecyclableMemoryStreamManager m = new();

    static async Task<Stream> CanonicalRequest(HttpRequestMessage req, IList<string> signedHeaders, FormUrlEncodedContent body, CancellationToken cancellationToken)
    {
        var bodyStream = await body.ReadAsStreamAsync(cancellationToken);
        Span<byte> bodyHash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(bodyStream, bodyHash);
        var hexencode = Convert.ToHexStringLower(bodyHash);
        var s = m.GetStream();
        s.WriteU8String(req.Method.ToString());
        s.Write("\n"u8);
        var requestUri = req.RequestUri!.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped);
        CanonicalURI();
        s.Write("\n"u8);
        // CanonicalQueryString
        s.Write("\n"u8);
        void CanonicalURI()
        {
            var pattens = requestUri!.Split('/');
            foreach (var v in pattens)
            {
                s.WriteU8StringUrlEncode(v);
                s.Write("/"u8);
            }
        }
        CanonicalHeaders();
        s.Write("\n"u8);
        void CanonicalHeaders()
        {
            foreach (string key in signedHeaders)
            {
                if (req.Headers!.TryGetValues(key, out var headerValues))
                {
                    SortedList<string, string> values = new(StringComparer.Ordinal);
                    foreach (var it in headerValues)
                    {
                        values.Add(it, it);
                    }
                    foreach (var value in values.Keys)
                    {
                        s.WriteU8String(key);
                        s.Write(":"u8);
                        s.WriteU8StringTrim(value);
                        s.Write("\n"u8);
                        req.Headers.Remove(key);
                        req.Headers.TryAddWithoutValidation(key, Encoding.GetEncoding("iso-8859-1").GetString(Encoding.UTF8.GetBytes(value)));
                    }
                }
            }
        }

        for (int i = 0; i < signedHeaders.Count; i++)
        {
            s.WriteU8String(signedHeaders[i]);
            if (i != signedHeaders.Count - 1)
            {
                s.Write(";"u8);
            }
        }

        s.Write("\n"u8);
        s.WriteU8String(hexencode);
        return s;
    }

    static RecyclableMemoryStream StringToSign(Stream canonicalRequest, DateTime time)
    {
        canonicalRequest.Position = 0;
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(canonicalRequest, hash);
        var s = m.GetStream();
        s.WriteU8String(Algorithm);
        s.Write("\n"u8);
        s.WriteU8String(time.ToUniversalTime().ToString(BasicDateFormat));
        s.Write("\n"u8);
        Span<char> hashHex = stackalloc char[SHA256.HashSizeInBytes * 2];
        Convert.TryToHexStringLower(hash, hashHex, out var charsWritten);
        hashHex = hashHex[..charsWritten];
        s.WriteU8String(hashHex);
        return s;
    }

    static string SignStringToSign(RecyclableMemoryStream stringToSign, ReadOnlySpan<byte> signingKey)
    {
        stringToSign.Position = 0;
        var stringToSignSpan = stringToSign.GetBuffer().AsSpan(0, unchecked((int)stringToSign.Position));

        Span<byte> hash = stackalloc byte[HMACSHA256.HashSizeInBytes];
        HMACSHA256.HashData(stringToSignSpan, stringToSign, hash);

        var r = Convert.ToHexStringLower(hash);
        return r;
    }

    static string AuthHeaderValue(string signature, string appKey, IList<string> signedHeaders)
    {
        using var s = m.GetStream();
        s.WriteU8String(Algorithm);
        s.Write(" Access="u8);
        s.WriteU8String(appKey);
        s.Write(", SignedHeaders="u8);
        for (int i = 0; i < signedHeaders.Count; i++)
        {
            s.WriteU8String(signedHeaders[i]);
            if (i != signedHeaders.Count - 1)
            {
                s.Write(";"u8);
            }
        }
        s.Write(", Signature="u8);
        s.WriteU8String(signature);
        var r = Encoding.UTF8.GetString(s.GetBuffer().AsSpan(0, unchecked((int)s.Length)));
        return r;
    }

    #endregion

    /// <inheritdoc/>
    public override async Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken)
    {
        var appKey = options.AppKey;
        ArgumentNullException.ThrowIfNull(appKey);
        var sender = options.Sender;
        ArgumentNullException.ThrowIfNull(sender);
        var apiAddress = options.ApiAddress;
        ArgumentNullException.ThrowIfNull(apiAddress);
        var appSecret = options.AppSecret;
        ArgumentNullException.ThrowIfNull(appSecret);
        var templateId = options.Templates?.FirstOrDefault(x => x.Type == type)?.Template ?? options.DefaultTemplate;
        ArgumentNullException.ThrowIfNull(templateId);
        var statusCallback = options.StatusCallBack;
        ArgumentNullException.ThrowIfNull(statusCallback);
        var signature = options.Signature;
        ArgumentNullException.ThrowIfNull(signature);

        var templateParam = $"[\"{message}\"]";
        var body = new Dictionary<string, string>() {
            { "from", sender }, //短信发送方的号码
            { "to", number }, //短信接收方的号码
            { "templateId", templateId }, //短信模板 ID，用于唯一标识短信模板，请在申请短信模板时获取模板 ID
            { "templateParas", templateParam }, //短信模板的变量值列表
            { "statusCallback", statusCallback }, //用户的回调地址
            { "signature", signature } //使用国内短信通用模板时,必须填写签名名称
        };

        var request = await Sign(appKey, appSecret, apiAddress, new FormUrlEncodedContent(body), cancellationToken);

        var response = await httpClient.SendAsync(request, cancellationToken);
        var isSuccess = false;
        SendHuaweiCloudResult? jsonObject = null;

        if (response.IsSuccessStatusCode)
        {
            jsonObject = await ReadFromJsonAsync(response.Content, SmsSenderJsonSerializerContext.Default.SendHuaweiCloudResult, cancellationToken);
            isSuccess = jsonObject != default && jsonObject.IsOK();
        }

        var result = new SendSmsResult<SendHuaweiCloudResult>
        {
            HttpStatusCode = (int)response.StatusCode,
            IsSuccess = isSuccess,
            Result = jsonObject,
            ResultObject = jsonObject
        };

        if (!result.IsSuccess)
        {
            SendSmsError(logger, IPhoneNumber.ToStringHideMiddleFour(number), message, type, result.HttpStatusCode);
        }
        return result;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message =
"""
调用华为云短信接口失败，手机号码：{phoneNumber}，短信内容：{message}，短信类型：{type}，HTTP 响应状态码：{httpStatusCode}
""")]
    private static partial void SendSmsError(ILogger logger, string phoneNumber, string? message, ushort type, int httpStatusCode);

    /// <inheritdoc/>
    public override Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

file static class _
{
    internal static void WriteU8String(this Stream stream, string? value)
    {
        if (value == null)
        {
            return;
        }

        WriteU8String(stream, value.AsSpan());
    }

    internal static void WriteU8String(this Stream stream, ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            return;
        }

        var len = Encoding.UTF8.GetMaxByteCount(value.Length);
        var b = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            Encoding.UTF8.TryGetBytes(value, b, out var bytesWritten);
            var span = b.AsSpan(0, bytesWritten);
            stream.Write(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(b);
        }
    }

    internal static void WriteU8StringTrim(this Stream stream, ReadOnlySpan<char> value)
    {
        WriteU8String(stream, value.Trim());
    }

    internal static void WriteU8StringUrlEncode(this Stream stream, string? value)
    {
        if (value == null)
        {
            return;
        }

        var b = HttpUtility.UrlEncodeToBytes(value);
        stream.Write(b);
    }
}