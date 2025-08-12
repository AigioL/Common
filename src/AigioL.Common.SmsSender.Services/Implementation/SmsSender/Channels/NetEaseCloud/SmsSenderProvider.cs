using AigioL.Common.Primitives.Columns;
using AigioL.Common.SmsSender.Models;
using AigioL.Common.SmsSender.Models.Abstractions;
using AigioL.Common.SmsSender.Models.Channels.NetEaseCloud;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CSR = AigioL.Common.SmsSender.Models.CheckSmsResult<AigioL.Common.SmsSender.Models.Channels.NetEaseCloud.NetEaseCloudResult>;
using SmsOptions = AigioL.Common.SmsSender.Models.Channels.NetEaseCloud.SmsNetEaseCloudOptions;
using SSR = AigioL.Common.SmsSender.Models.SendSmsResult<AigioL.Common.SmsSender.Models.Channels.NetEaseCloud.SendSmsNetEaseCloudResult>;
using SSRR = AigioL.Common.SmsSender.Models.Channels.NetEaseCloud.SendSmsNetEaseCloudResult;

/*
返回说明
http 响应:json
发送成功则返回相关信息。msg字段表示此次发送的sendid；obj字段表示此次发送的验证码。
"Content-Type": "application/json; charset=utf-8"
{
"code": 200,
"msg": "88",
"obj": "1908"
}
主要的返回码
200、315、403、414、416、500
200	操作成功
315	IP限制
403	非法操作或没有权限
414	参数错误
416	频率控制
500	服务器内部错误
 */

namespace AigioL.Common.SmsSender.Services.Implementation.SmsSender.Channels.NetEaseCloud;

/// <summary>
/// 短信服务（网易云实现） 需要实现校验接口
/// <para>参考资料：</para>
/// <para>http://dev.netease.im/docs/product/%E7%9F%AD%E4%BF%A1/%E7%9F%AD%E4%BF%A1%E6%8E%A5%E5%8F%A3%E6%8C%87%E5%8D%97</para>
/// <para>http://dev.netease.im/docs/product/IM%E5%8D%B3%E6%97%B6%E9%80%9A%E8%AE%AF/%E6%9C%8D%E5%8A%A1%E7%AB%AFAPI%E6%96%87%E6%A1%A3?#接口概述</para>
/// </summary>
public partial class SmsSenderProvider : SmsSenderBase, ISmsSender
{
    /// <summary>
    /// 网易云的名称
    /// </summary>
    public const string Name = nameof(NetEaseCloud);

    /// <inheritdoc/>
    public override string Channel => Name;

    /// <inheritdoc/>
    public override bool SupportCheck => true;

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

    /// <summary>
    /// 接口地址
    /// </summary>
    const string SmsSendApiUrl = "https://api.netease.im/sms/sendcode.action";

    /// <summary>
    /// 短信验证接口的 URL
    /// </summary>
    const string SmsCheckApiUrl = "https://api.netease.im/sms/verifycode.action";

    #endregion 常量

    #region helpers

    /// <summary>
    /// 当前 UTC 时间戳，秒数
    /// </summary>
    /// <returns></returns>
    static string CurTime() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    /// <summary>
    /// 生成唯一标识符
    /// </summary>
    static string Nonce() => Guid.NewGuid().ToString("N") + DateTime.Now.Ticks;

    static string CheckSum(string appSecret, string nonce, string curTime)
    {
        string result;
        var len = Encoding.UTF8.GetMaxByteCount(appSecret.Length + nonce.Length + curTime.Length);
        var b = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            len = 0;
            if (!Encoding.UTF8.TryGetBytes(appSecret, b, out var bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode AppSecret to UTF-8.");
            }
            len += bytesWritten;
            if (!Encoding.UTF8.TryGetBytes(nonce, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode Nonce to UTF-8.");
            }
            len += bytesWritten;
            if (!Encoding.UTF8.TryGetBytes(curTime, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode CurTime to UTF-8.");
            }
            len += bytesWritten;
            Span<byte> hash = new byte[SHA1.HashSizeInBytes];
            SHA1.HashData(b.AsSpan(0, len), hash);
            result = Convert.ToHexString(hash);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(b);
        }
        return result;
    }

    /// <summary>
    /// 请求网易云接口
    /// </summary>
    async Task<T> PostAsync<T, TResult>(
        string requestUri,
        Dictionary<string, string?> args,
        JsonTypeInfo<TResult> jsonTypeInfo,
        CancellationToken cancellationToken)
        where T : SmsResult<TResult, T>, IJsonSerializerContext, new()
        where TResult : NetEaseCloudResult<TResult>
    {
        var appKey = options.AppKey;
        ArgumentNullException.ThrowIfNull(appKey);
        var appSecret = options.AppSecret;
        ArgumentNullException.ThrowIfNull(appSecret);
        var nonce = Nonce();
        var curTime = CurTime();
        var checkSum = CheckSum(appSecret, nonce, curTime);
        var content = new FormUrlEncodedContent(args);
        content.Headers.ContentType = new("application/x-www-form-urlencoded", "utf-8");
        // ↑ application/x-www-form-urlencoded;charset=utf-8

        var request_args = new
        {
            body = args,
            headers = new Dictionary<string, string>
            {
                { nameof(options.AppKey), appKey },
                { nameof(Nonce), nonce },
                { nameof(CurTime), curTime },
                { "CheckSum", checkSum },
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content,
        };

        foreach (var header in request_args.headers)
            request.Headers.Add(header.Key, header.Value);

        var isSuccess = false;
        TResult? jsonObject = null;
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            jsonObject = await ReadFromJsonAsync(response.Content, jsonTypeInfo, cancellationToken);
            isSuccess = jsonObject != default && jsonObject.IsOK();
        }

        return new T
        {
            HttpStatusCode = (int)response.StatusCode,
            IsSuccess = isSuccess,
            Result = jsonObject,
            ResultObject = jsonObject,
        };
    }

    #endregion helpers

    /// <inheritdoc/>
    public override async Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken)
    {
        var dictionary = new Dictionary<string, string?> { { "mobile", number } };
        var template_id = options.Templates?.FirstOrDefault(x => x.Type == type)?.Template ?? options.DefaultTemplate;
        if (template_id.HasValue)
            dictionary.Add("templateid", template_id.Value.ToString());
        dictionary.Add("authCode", message);

        var result = await PostAsync<SSR, SSRR>(SmsSendApiUrl, dictionary, SmsSenderJsonSerializerContext.Default.SendSmsNetEaseCloudResult, cancellationToken);
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
调用网易云短信接口失败，手机号码：{phoneNumber}，短信内容：{message}，短信类型：{type}，HTTP 响应状态码：{httpStatusCode}
""")]
    private static partial void SendSmsError(ILogger logger, string phoneNumber, string? message, ushort type, int httpStatusCode);

    /// <inheritdoc/>
    public override async Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken)
    {
        var dictionary = new Dictionary<string, string?> { { "mobile", number }, { "code", message } };

        var result = await PostAsync<CSR, NetEaseCloudResult>(SmsCheckApiUrl, dictionary, SmsSenderJsonSerializerContext.Default.NetEaseCloudResult, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Result != default && result.Result.IsCheckSmsFail())
            {
                result.IsCheckSuccess = true;
            }
            else
            {
                CheckSmsError(logger, IPhoneNumber.ToStringHideMiddleFour(number), message, result.HttpStatusCode);
            }
        }

        return result;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message =
"""
调用网易云短信验证接口失败，手机号码：{phoneNumber}，短信内容：{message}，HTTP 响应状态码：{httpStatusCode}
""")]
    private static partial void CheckSmsError(ILogger logger, string phoneNumber, string? message, int httpStatusCode);
}