using AigioL.Common.Primitives.Columns;
using AigioL.Common.SmsSender.Models;
using AigioL.Common.SmsSender.Models.Abstractions;
using AigioL.Common.SmsSender.Models.Channels.TencentCloud;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    #region 常量

    private const string Endpoint = "https://sms.tencentcloudapi.com";
    private const string Version = "2021-01-11";
    private const string SdkVersion = "SDK_NET_3.0.1207";
    private const string Action = "SendSms";
    private const string Method = "POST";
    private const string ContentType = "application/json";

    #endregion

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

    private Dictionary<string, string> BuildHeaders(Stream requestPayload)
    {
        // https://github.com/TencentCloud/tencentcloud-sdk-dotnet/blob/8a2d9b3e0247eb258058d8a557e5f2e08cdb6b34/TencentCloud/Common/AbstractClient.cs#L302

        string endpoint = Endpoint;
        string httpRequestMethod = Method;
        string contentType = ContentType;
        string canonicalQueryString = "";

        string canonicalURI = "/";
        string canonicalHeaders = "content-type:" + contentType + "\nhost:" + endpoint + "\n";
        string signedHeaders = "content-type;host";

        Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(requestPayload, hashBytes);
        StringBuilder builder = new StringBuilder();
        string hashedRequestPayload = Convert.ToHexStringLower(hashBytes);

        string algorithm = "TC3-HMAC-SHA256";
        long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        string requestTimestamp = timestamp.ToString();
        string date = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToString("yyyy-MM-dd");
        string service = endpoint.Split('.')[0];
        string credentialScope = date + "/" + service + "/" + "tc3_request";
        string hashedCanonicalRequest = GetHashedCanonicalRequest(httpRequestMethod, canonicalURI, canonicalQueryString, canonicalHeaders, signedHeaders, hashedRequestPayload);
        string stringToSign = algorithm + "\n"
                                        + requestTimestamp + "\n"
                                        + credentialScope + "\n"
                                        + hashedCanonicalRequest;

        byte[] tc3SecretKey = Encoding.UTF8.GetBytes("TC3" + options.SecretKey);
        byte[] secretDate = HMACSHA256.HashData(tc3SecretKey, Encoding.UTF8.GetBytes(date));
        byte[] secretService = HMACSHA256.HashData(secretDate, Encoding.UTF8.GetBytes(service));
        byte[] secretSigning = HMACSHA256.HashData(secretService, Encoding.UTF8.GetBytes("tc3_request"));
        byte[] signatureBytes = HMACSHA256.HashData(secretSigning, Encoding.UTF8.GetBytes(stringToSign));
        string signature = Convert.ToHexStringLower(signatureBytes);

        string authorization = algorithm + " "
                                         + "Credential=" + options.SecretId + "/" + credentialScope + ", "
                                         + "SignedHeaders=" + signedHeaders + ", "
                                         + "Signature=" + signature;

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", authorization);
        headers.Add("Host", endpoint);
        headers.Add("Content-Type", contentType);
        headers.Add("X-TC-Timestamp", requestTimestamp);
        headers.Add("X-TC-Version", Version);
        headers.Add("X-TC-Region", "");
        headers.Add("X-TC-RequestClient", SdkVersion);
        headers.Add("X-TC-Language", "zh-CN");
        headers.Add("X-TC-Action", Action);

        return headers;
    }

    private HttpRequestMessage GenerateHttpRequestMessage(string number, string message, string templateId)
    {
        // https://cloud.tencent.com/document/api/382/55981

        var params_dic = new JsonObject();
        params_dic.Add("Action", Action);
        params_dic.Add("Version", Version);
        params_dic.Add("Region", "");
        params_dic.Add("SmsSdkAppId", options.SmsSdkAppId!);
        params_dic.Add("TemplateId", templateId);
        params_dic.Add("SignName", options.SignName!);
        params_dic.Add("PhoneNumberSet", new JsonArray(JsonValue.Create(number)));
        params_dic.Add("TemplateParamSet", new JsonArray(JsonValue.Create(message)));

        MemoryStream requestPayload = new();
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        JsonSerializer.Serialize(requestPayload, params_dic, SmsSenderJsonSerializerContext.Default.Options);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        var headers = BuildHeaders(requestPayload);

        var requestUri = $"{Endpoint}/{Action}";
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);

        foreach (KeyValuePair<string, string> kvp in headers)
        {
            if (kvp.Key.Equals("Content-Type"))
            {
                requestPayload.Position = 0; // 重置流位置，以便读取内容
                var content = new StreamContent(requestPayload);
                content.Headers.Remove("Content-Type");
                content.Headers.Add("Content-Type", kvp.Value);
                requestMessage.Content = content;
            }
            else if (kvp.Key.Equals("Host"))
            {
                requestMessage.Headers.Host = kvp.Value;
            }
            else if (kvp.Key.Equals("Authorization"))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("TC3-HMAC-SHA256",
                    kvp.Value["TC3-HMAC-SHA256".Length..]);
            }
            else
            {
                requestMessage.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }
        }

        return requestMessage;
    }

    /// <inheritdoc/>
    public override async Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken)
    {
        var template_code = options.Templates?.FirstOrDefault(x => x.Type == type)?.Template ?? options.DefaultTemplate;
        ArgumentNullException.ThrowIfNull(template_code);

        using var request = GenerateHttpRequestMessage(number, message, template_code);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var isSuccess = false;
        TencentCloudResult<SendSmsTencentCloudResult>? tencentCloudResult = null;

        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            tencentCloudResult = await JsonSerializer.DeserializeAsync(stream, SmsSenderJsonSerializerContext.Default.TencentCloudResultSendSmsTencentCloudResult, cancellationToken);

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
