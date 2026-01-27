using AigioL.Common.Extensions.Http.Models;
using Microsoft.IO;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

public static partial class HttpResponseMessageExtensions
{
    /// <summary>
    /// 是否为重定向状态码
    /// </summary>
    public static bool IsRedirectStatusCode(this HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.MovedPermanently or
        HttpStatusCode.Redirect or
        HttpStatusCode.RedirectKeepVerb or
        HttpStatusCode.PermanentRedirect => true,
        _ => false,
    };

    /// <summary>
    /// 是否为重定向状态码
    /// </summary>
    public static bool IsRedirectStatusCode(this HttpResponseMessage response)
        => response.StatusCode.IsRedirectStatusCode();

    static readonly RecyclableMemoryStreamManager m = new();

    /// <summary>
    /// 从响应中以调试友好的方式读取流
    /// </summary>
    public static async Task<Stream> ReadAsStreamExAsync(
        this HttpResponseMessage response,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        RecyclableMemoryStream? memoryStream = null;
        if (responseMessageRecord != null)
        {
            memoryStream = responseMessageRecord.Content;
            responseMessageRecord.SetResponseMessage(response);
        }

        Stream? disposable = null;
        try
        {
            var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            if (memoryStream != null)
            {
                // 如果传入了内存流，则将内容复制到内存流
                memoryStream.Position = memoryStream.Length; // 移动到末尾以追加内容
                await contentStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                disposable = contentStream;
            }

#if DEBUG
            string html = null!;
            bool debugUseMemoryStream = false; // 调试时断点将此值改为 true 查看 HTML 内容，因仅调试查看，不管释放流
            if (debugUseMemoryStream)
            {
                var requestUri = response.RequestMessage?.RequestUri;

                if (memoryStream == null)
                {
                    memoryStream = m.GetStream();
                    await contentStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                    disposable = contentStream;
                }

                html = Encoding.UTF8.GetString(memoryStream.GetReadOnlySequence()); // 仅调试模式查看 HTML 内容
            }
#endif

            if (memoryStream != null)
            {
                memoryStream.Position = 0;
                return memoryStream;
            }
            return contentStream;
        }
        finally
        {
            disposable?.Dispose();
        }
    }

    /// <summary>
    /// 从响应中读取 JSON，支持调试时查看内容
    /// </summary>
    public static async Task<T?> ReadFromJsonExAsync<T>(
        this HttpResponseMessage response,
        JsonTypeInfo<T> jsonTypeInfo,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        RecyclableMemoryStream? memoryStream = null;
        if (responseMessageRecord != null)
        {
            memoryStream = responseMessageRecord.Content;
            responseMessageRecord.SetResponseMessage(response);
        }

#if DEBUG
        IDisposable? disposable = null;
        string json = null!;
        bool debugUseMemoryStream = false; // 调试时断点将此值改为 true 查看 JSON 内容，因仅调试查看，不管释放流
        if (debugUseMemoryStream)
        {
            disposable = memoryStream = m.GetStream();
        }
#endif
        try
        {
            if (memoryStream != null)
            {
                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                memoryStream.Position = memoryStream.Length; // 移动到末尾以追加内容
                await contentStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;
            }

#if DEBUG
            if (debugUseMemoryStream)
            {
                var requestUri = response.RequestMessage?.RequestUri;

                json = Encoding.UTF8.GetString(memoryStream!.GetReadOnlySequence()); // 仅调试模式查看内容
                memoryStream.Position = 0;
                response.Content = new StreamContent(memoryStream);
            }
#endif

            T? obj;
            if (memoryStream != null)
            {
                // https://github.com/dotnet/runtime/blob/v10.0.2/src/libraries/System.Net.Http.Json/src/System/Net/Http/Json/HttpContentJsonExtensions.cs#L128
                obj = await JsonSerializer.DeserializeAsync(
                    memoryStream,
                    jsonTypeInfo,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                obj = await response.Content.ReadFromJsonAsync(
                    jsonTypeInfo,
                    cancellationToken);
            }
            return obj;
        }
        finally
        {
#if DEBUG
            disposable?.Dispose();
#endif
        }
    }

    /// <summary>
    /// 当 <see cref="HttpResponseMessage.IsSuccessStatusCode"/> 为 <see langword="false"/> 时，记录日志
    /// </summary>
    public static async Task LogIsFailStatusCodeAsync(
        this HttpResponseMessage response,
        ILogger logger,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            // 状态码成功时不执行操作
            return;
        }

        responseMessageRecord?.SetResponseMessage(response);

        Exception? exception = null;
        var requestUri = response.RequestMessage == null ? null : HttpRequestMessageRecord.GetOriginalRequestUri(response.RequestMessage);
        var requestUriHide = HttpRequestMessageRecord.GetRequestUriHideQuery(requestUri);

        var method = response.RequestMessage?.Method?.Method;
        string? responseBody = null;
        if (response.Content != null)
        {
            try
            {
                if (responseMessageRecord?.Content != null)
                {
                    responseBody = responseMessageRecord.ReadAsString();
                }
                else
                {
                    responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        LogHttpResponseIsFailStatusCode(logger, exception, unchecked((int)response.StatusCode), requestUriHide, method, responseBody);
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message =
            "发送 HTTP 请求时，响应状态码失败：{statusCode}，请求地址：{requestUriHide}，请求方法：{method}，响应正文：{responseBody}")]
    private static partial void LogHttpResponseIsFailStatusCode(
        ILogger logger, Exception? exception, int statusCode, string? requestUriHide,
        string? method, string? responseBody);
}

static partial class HttpResponseMessageExtensions
{
    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v10.0.2/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/CookieHelper.cs
    /// </summary>
    public static void ProcessReceivedCookies(this HttpResponseMessage response, CookieContainer cookieContainer)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values))
        {
            // The header values are always a string[]
            var valuesArray = (string[])values;
            Debug.Assert(valuesArray.Length > 0, "No values for header??");
            Debug.Assert(response.RequestMessage != null && response.RequestMessage.RequestUri != null);

            Uri requestUri = response.RequestMessage.RequestUri;
            for (int i = 0; i < valuesArray.Length; i++)
            {
                try
                {
                    cookieContainer.SetCookies(requestUri, valuesArray[i]);
                }
                catch (CookieException)
                {
                    // Ignore invalid Set-Cookie header and continue processing.
                    //if (NetEventSource.Log.IsEnabled())
                    //{
                    //    NetEventSource.Error(response, $"Invalid Set-Cookie '{valuesArray[i]}' ignored.");
                    //}
                }
            }
        }
    }
}