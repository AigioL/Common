using AigioL.Common.Extensions.Http.Converters;
using Microsoft.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Extensions.Http.Models;

/// <summary>
/// 记录 <see cref="HttpResponseMessage"/> 的模型类
/// </summary>
public partial class HttpResponseMessageRecord
{
    /// <summary>
    /// 响应内容流，不为 null 时 Send 则将内容复制到该流中
    /// </summary>
    [JsonConverter(typeof(RecyclableMemoryStreamToStringJsonConverter))]
    public RecyclableMemoryStream? Content { get; set; }

    /// <inheritdoc cref="HttpContentHeaders.ContentType"/>
    [JsonConverter(typeof(MediaTypeHeaderValueToStringJsonConverter))]
    public MediaTypeHeaderValue? ContentType { get; set; }

    /// <inheritdoc cref="HttpContentHeaders.ContentLength"/>
    public long? ContentLength { get; set; }

    //public Dictionary<string, string>? Headers { get; set; } 不记录头部信息

    /// <inheritdoc cref="HttpResponseMessage.StatusCode"/>
    public HttpStatusCode StatusCode { get; set; }

    /// <inheritdoc cref="HttpResponseMessage.Version"/>
    public Version? Version { get; set; }
}

partial class HttpResponseMessageRecord
{
    /// <inheritdoc cref="HttpRequestMessage.Version"/>
    public HttpRequestMessageRecord? Request { get; set; }
}

partial class HttpResponseMessageRecord
{
    protected static readonly RecyclableMemoryStreamManager m = new();

    /// <summary>
    /// 创建一个仅记录内容流的实例
    /// </summary>
    /// <returns></returns>
    public static HttpResponseMessageRecord CreateContentOnly() => new()
    {
        Content = m.GetStream(),
    };

    protected virtual HttpRequestMessageRecord CreateHttpRequestMessageRecord(HttpRequestMessage request)
    {
        HttpRequestMessageRecord r = new();
        r.SetRequestMessage(request);
        return r;
    }

    public virtual void SetResponseMessage(HttpResponseMessage response)
    {
        StatusCode = response.StatusCode;
        Version = response.Version;
        ContentType = response.Content?.Headers.ContentType;
        ContentLength = response.Content?.Headers.ContentLength;

        var request = response.RequestMessage;
        if (request != null)
        {
            Request = CreateHttpRequestMessageRecord(request);
        }

        // 这里不赋值 Content，由调用 XXXExAsync 时赋值响应正文内容
    }

    /// <inheritdoc cref="HttpContent.ReadAsStringAsync()"/>
    public virtual string? ReadAsString(Encoding? encoding = null)
    {
        if (Content == null)
        {
            return null;
        }

        encoding ??= Encoding.UTF8;
        var r = encoding.GetString(Content.GetReadOnlySequence());
        return r;
    }

    public override string ToString()
    {
        var str = JsonSerializer.Serialize(this, ExHttpJsonSerializerContext.Default.HttpResponseMessageRecord);
        return str;
    }
}

partial class HttpResponseMessageRecord : IDisposable
{
    bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                Content?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            Content = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}