using AigioL.Common.FeishuOApi.Sdk.Models;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AigioL.Common.FeishuOApi.Sdk.Services;

/// <summary>
/// 飞书开放平台 WebApi 调用实现
/// </summary>
/// <param name="logger"></param>
/// <param name="httpClient"></param>
/// <param name="options"></param>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed partial class FeishuApiClient(ILogger<FeishuApiClient> logger, HttpClient httpClient, IOptions<FeishuApiOptions> options) : IFeishuApiClient
{
    string DebuggerDisplay() => $"FeishuApiClient, HookId: {HookId}, ServerTag: {options.Value.ServerTag}";

    /// <inheritdoc cref="FeishuApiOptions.HookId"/>
    string HookId
    {
        get
        {
            var hookId = options.Value.HookId;
            if (string.IsNullOrWhiteSpace(hookId))
                throw GetInvalidKeyException(nameof(HookId));
            return hookId!;
        }
    }

    static ApplicationException GetInvalidKeyException(string name) => throw new(
$"""
Enter (dotnet user-secrets set "FeishuApiOptions:{name}" "value") on the current csproj path to set the secret value see https://learn.microsoft.com/zh-cn/aspnet/core/security/app-secrets
""");

    public async Task<ApiRsp> SendMessageAsync(
        string? title,
        string? text,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage? response = null;
        try
        {
            var serverTag = options.Value.ServerTag;
            if (!string.IsNullOrWhiteSpace(serverTag))
            {
                text = $"{serverTag}: {text}";
            }

            Uri requestUri = new Uri($"/open-apis/bot/v2/hook/{HookId}", UriKind.Relative);
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var richTextMessage = GetRichTextMessage(title, text, out var bytesWritten);
            try
            {
                request.Content = new ByteArrayContent(richTextMessage, 0, bytesWritten)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json"),
                    },
                };
                response = await httpClient.SendAsync(request, cancellationToken);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(richTextMessage);
            }
            return response.StatusCode;
        }
        catch (Exception ex)
        {
            string? rspContent = null;
            try
            {
                if (response?.Content != null)
                {
                    rspContent = await response.Content.ReadAsStringAsync(cancellationToken);
                }
            }
            catch
            {
            }

            OnSendMessageAsyncFail(logger, ex, title, text, rspContent);
            return ex;
        }
        finally
        {
            response?.Dispose();
        }
    }

    internal static byte[] GetRichTextMessage(string? title, string? text, out int bytesWritten)
    {
        var json_0 =
"""
{
  "msg_type": "post",
  "content": {
    "post": {
      "zh_cn": {
        "title": "
"""u8;
        var titleU8 = title == null ? [] : JsonEncodedText.Encode(title, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).EncodedUtf8Bytes;
        var json_1 =
"""
",
        "content": [
          [
            {
              "tag": "text",
              "text": "
"""u8;
        var textU8 = text == null ? [] : JsonEncodedText.Encode(text, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).EncodedUtf8Bytes;
        var json_2 =
"""
"
            }
          ]
        ]
      }
    }
  }
}
"""u8;
        bytesWritten = json_0.Length + titleU8.Length + json_1.Length + textU8.Length + json_2.Length;
        var buffer = ArrayPool<byte>.Shared.Rent(bytesWritten);
        json_0.CopyTo(buffer);
        titleU8.CopyTo(buffer.AsSpan(json_0.Length));
        json_1.CopyTo(buffer.AsSpan(json_0.Length + titleU8.Length));
        textU8.CopyTo(buffer.AsSpan(json_0.Length + titleU8.Length + json_1.Length));
        json_2.CopyTo(buffer.AsSpan(json_0.Length + titleU8.Length + json_1.Length + textU8.Length));
        return buffer;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "SendMessageAsync fail, title: {title}, text:{text}, rspContent: {rspContent}")]
    private static partial void OnSendMessageAsyncFail(ILogger logger, Exception exception, string? title, string? text, string? rspContent);
}