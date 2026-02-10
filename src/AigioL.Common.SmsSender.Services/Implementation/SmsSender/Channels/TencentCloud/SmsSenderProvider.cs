using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.SmsSender.Models;
using AigioL.Common.SmsSender.Models.Abstractions;
using AigioL.Common.SmsSender.Models.Channels.TencentCloud;
using Microsoft.IO;
using System.Buffers;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmsOptions = AigioL.Common.SmsSender.Models.Channels.TencentCloud.SmsTencentCloudOptions;

namespace AigioL.Common.SmsSender.Services.Implementation.SmsSender.Channels.TencentCloud;

/// <summary>
/// 短信服务提供商 - 腾讯云
/// </summary>
public partial class SmsSenderProvider : SmsSenderBase, ISmsSender
{
    /// <summary>
    /// 阿里云的名称
    /// </summary>
    public const string Name = nameof(TencentCloud);

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

    static string GetHashedCanonicalRequest(string httpRequestMethod, string canonicalURI, string canonicalQueryString, string canonicalHeaders, string signedHeaders, string hashedRequestPayload)
    {
        string result;
        var len = Encoding.UTF8.GetMaxByteCount(httpRequestMethod.Length + 1 + canonicalURI.Length + 1 + canonicalQueryString.Length + 1 + canonicalHeaders.Length + 1 + signedHeaders.Length + 1 + hashedRequestPayload.Length);
        var b = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            len = 0;
            if (!Encoding.UTF8.TryGetBytes(httpRequestMethod, b, out var bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode httpRequestMethod to UTF-8.");
            }
            len += bytesWritten;
            b.AsSpan(len)[0] = 10; // Encoding.UTF8.GetBytes("\n") = 10
            len += 1;
            if (!Encoding.UTF8.TryGetBytes(canonicalURI, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode canonicalURI to UTF-8.");
            }
            len += bytesWritten;
            b.AsSpan(len)[0] = 10; // Encoding.UTF8.GetBytes("\n") = 10
            len += 1;
            if (!Encoding.UTF8.TryGetBytes(canonicalQueryString, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode canonicalQueryString to UTF-8.");
            }
            len += bytesWritten;
            b.AsSpan(len)[0] = 10; // Encoding.UTF8.GetBytes("\n") = 10
            len += 1;
            if (!Encoding.UTF8.TryGetBytes(canonicalHeaders, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode canonicalHeaders to UTF-8.");
            }
            len += bytesWritten;
            b.AsSpan(len)[0] = 10; // Encoding.UTF8.GetBytes("\n") = 10
            len += 1;
            if (!Encoding.UTF8.TryGetBytes(signedHeaders, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode signedHeaders to UTF-8.");
            }
            len += bytesWritten;
            b.AsSpan(len)[0] = 10; // Encoding.UTF8.GetBytes("\n") = 10
            len += 1;
            if (!Encoding.UTF8.TryGetBytes(hashedRequestPayload, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode hashedRequestPayload to UTF-8.");
            }
            len += bytesWritten;
            Span<byte> hash = new byte[SHA256.HashSizeInBytes];
            SHA256.HashData(b.AsSpan(0, len), hash);
            result = Convert.ToHexString(hash);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(b);
        }
        return result;
    }

    static readonly RecyclableMemoryStreamManager m = new();

    HttpRequestMessage GenerateHttpRequestMessage(Stream requestPayload, string number, string message, string templateId)
    {
        // https://cloud.tencent.com/document/api/382/55981
        // https://github.com/TencentCloud/tencentcloud-sdk-dotnet/blob/master/TencentCloud/Sms/V20210111/SmsClient.cs
        // 使用签名方法 v3 时，公共参数需要统一放到 HTTP Header 请求头部中

        var secretId = options.SecretId;
        ArgumentNullException.ThrowIfNull(secretId);
        var secretKey = options.SecretKey;
        ArgumentNullException.ThrowIfNull(secretKey);
        var smsSdkAppId = options.SmsSdkAppId;
        ArgumentNullException.ThrowIfNull(smsSdkAppId);

        var region = string.IsNullOrWhiteSpace(options.Region) ? region_广州_华南地区 : options.Region;
        var sendSmsRequest = new SendSmsRequest
        {
            PhoneNumberSet = [number],
            SmsSdkAppId = smsSdkAppId,
            TemplateId = templateId,
            SignName = options.SignName,
            TemplateParamSet = [message],
        };

        const string canonicalQueryString = "";
        requestPayload.Position = 0;
        JsonSerializer.Serialize(requestPayload, sendSmsRequest, TencentCloudSmsSenderJsonSerializerContext.Default.SendSmsRequest);
        requestPayload.SetLength(requestPayload.Position);

        var method = HttpMethod.Post;
        const string contentType = MediaTypeNames.JSON;
        var headers = BuildHeaders(
            secretId, secretKey, endpoint,
            method, contentType, requestPayload,
            canonicalQueryString,
            region: region);

        var requestUri = $"https://{endpoint}/";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        requestPayload.Position = 0; // 重置流位置，以便读取内容
        var content = new StreamContent(requestPayload);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = content;

        foreach (KeyValuePair<string, string> kvp in headers)
        {
            if (kvp.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
            }
            else
            {
                request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }
        }

        return request;
    }

    /// <inheritdoc/>
    public override async Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken)
    {
        var template_code = options.Templates?.FirstOrDefault(x => x.Type == type)?.Template ?? options.DefaultTemplate;
        ArgumentNullException.ThrowIfNull(template_code);

        using var requestPayload = m.GetStream();
        using var request = GenerateHttpRequestMessage(requestPayload, number, message, template_code);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var isSuccess = false;
        TencentCloudResult<SendSmsTencentCloudResult>? tencentCloudResult = null;

        if (response.IsSuccessStatusCode)
        {
#if DEBUG
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            tencentCloudResult = JsonSerializer.Deserialize(jsonString, SmsSenderJsonSerializerContext.Default.TencentCloudResultSendSmsTencentCloudResult);
#else
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            tencentCloudResult = await JsonSerializer.DeserializeAsync(stream, SmsSenderJsonSerializerContext.Default.TencentCloudResultSendSmsTencentCloudResult, cancellationToken);
#endif

            isSuccess =
                tencentCloudResult != null &&
                tencentCloudResult.Response.IsOk();
        }

        var result = new SendSmsResult<TencentCloudResult<SendSmsTencentCloudResult>>
        {
            HttpStatusCode = (int)response.StatusCode,
            IsSuccess = isSuccess,
            Result = tencentCloudResult,
            ResultObject = tencentCloudResult,
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
调用腾讯云短信接口失败，手机号码：{phoneNumber}，短信内容：{message}，短信类型：{type}，HTTP 响应状态码：{httpStatusCode}
""")]
    private static partial void SendSmsError(ILogger logger, string phoneNumber, string? message, ushort type, int httpStatusCode);

    /// <inheritdoc/>
    public override Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}

partial class SmsSenderProvider
{
    static string SHA256Hex(string s) => SHA256Hex(Encoding.UTF8.GetBytes(s));

    static string SHA256Hex(byte[] b)
    {
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(b, hash);
        var hex = Convert.ToHexStringLower(hash);
        return hex;
    }

    static string SHA256Hex(Stream s)
    {
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        s.Position = 0; // 重置流位置，以便读取内容
        SHA256.HashData(s, hash);
        var hex = Convert.ToHexStringLower(hash);
        return hex;
    }

    static string GetSplitFirst(ReadOnlySpan<char> s, char separator)
    {
        var split = s.Split(separator);
        split.MoveNext();
        return new(s[split.Current]);
    }

    internal const string endpoint = "sms.tencentcloudapi.com";
    internal const string action = "SendSms";
    internal const string version = "2021-01-11";
    internal const string sdkVersion = "SDK_NET_3.0.1207";
    internal const string region_广州_华南地区 = "ap-guangzhou";
    internal const string region_北京_华北地区 = "ap-beijing";
    internal const string region_南京_华东地区 = "ap-nanjing";

    internal static Dictionary<string, string> BuildHeaders(
        string secretId, string secretKey, string endpoint,
        HttpMethod method, string contentType, Stream requestPayload,
        string canonicalQueryString, string? action = action, string region = region_广州_华南地区,
        string version = version, string sdkVersion = sdkVersion)
    {
        // https://github.com/TencentCloud/tencentcloud-sdk-dotnet/blob/8a2d9b3e0247eb258058d8a557e5f2e08cdb6b34/TencentCloud/Common/AbstractClient.cs#L261
        // https://console.cloud.tencent.com/api/explorer?Product=sms&Version=2021-01-11&Action=SendSms&SignVersion=api3v3

        string canonicalURI = "/";
        string canonicalHeaders = "content-type:" + contentType + "\nhost:" + endpoint + "\n";
        //if (action != null)
        //{
        //    canonicalHeaders += "x-tc-action:" + action.ToLowerInvariant() + "\n";
        //}
        string signedHeaders = "content-type;host";
        //if (action != null)
        //{
        //    signedHeaders += ";x-tc-action";
        //}
        string hashedRequestPayload = SHA256Hex(requestPayload);
        // 拼接规范请求串
        string canonicalRequest = method.Method + "\n"
                                                + canonicalURI + "\n"
                                                + canonicalQueryString + "\n"
                                                + canonicalHeaders + "\n"
                                                + signedHeaders + "\n"
                                                + hashedRequestPayload;

        const string algorithm = "TC3-HMAC-SHA256";
        var now = DateTimeOffset.Now;
        long timestamp = now.ToUnixTimeSeconds();
        string requestTimestamp = timestamp.ToString();
        string date = now.ToString("yyyy-MM-dd");
        string service = GetSplitFirst(endpoint, '.');
        string credentialScope = date + "/" + service + "/" + "tc3_request";
        string hashedCanonicalRequest = SHA256Hex(canonicalRequest);
        // 拼接待签名字符串
        string stringToSign = algorithm + "\n"
                                        + requestTimestamp + "\n"
                                        + credentialScope + "\n"
                                        + hashedCanonicalRequest;

        byte[] tc3SecretKey = Encoding.UTF8.GetBytes("TC3" + secretKey);
        byte[] secretDate = HMACSHA256.HashData(tc3SecretKey, Encoding.UTF8.GetBytes(date));
        byte[] secretService = HMACSHA256.HashData(secretDate, Encoding.UTF8.GetBytes(service));
        byte[] secretSigning = HMACSHA256.HashData(secretService, "tc3_request"u8);
        byte[] signatureBytes = HMACSHA256.HashData(secretSigning, Encoding.UTF8.GetBytes(stringToSign));
        // 计算签名
        string signature = Convert.ToHexStringLower(signatureBytes);

        string authorization = algorithm + " "
                                         + "Credential=" + secretId + "/" + credentialScope + ", "
                                         + "SignedHeaders=" + signedHeaders + ", "
                                         + "Signature=" + signature;

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", authorization);
        headers.Add("Host", endpoint);
        headers.Add("Content-Type", contentType);
        headers.Add("X-TC-Timestamp", requestTimestamp);
        headers.Add("X-TC-Version", version);
        headers.Add("X-TC-Region", region);
        headers.Add("X-TC-RequestClient", sdkVersion);
        headers.Add("X-TC-Language", "zh-CN");
        if (action != null)
        {
            headers.Add("X-TC-Action", action);
        }

        return headers;
    }
}