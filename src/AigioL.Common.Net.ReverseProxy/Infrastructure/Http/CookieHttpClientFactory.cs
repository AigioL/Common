using AigioL.Common.Net.ReverseProxy.Services.Abstractions;
using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

sealed class CookieHttpClientFactory : ICookieHttpClientFactory, IDisposable
{
    CookieContainer? cookieContainer;
    HttpClient? httpClient;
    bool disposedValue;

    public CookieHttpClientFactory()
    {
        cookieContainer = new();
        httpClient = CreateHttpClient();
    }

    public CookieContainer GetCookieContainer()
    {
        ObjectDisposedException.ThrowIf(disposedValue || cookieContainer == null, this);
        return cookieContainer;
    }

    public HttpClient CreateClient(string name)
    {
        ObjectDisposedException.ThrowIf(disposedValue || httpClient == null, this);
        return httpClient;
    }

    HttpMessageHandler CreateHttpHandler()
    {
        SocketsHttpHandler h = new()
        {
            UseCookies = true,
            CookieContainer = cookieContainer,
        };
        return h;
    }

    HttpClient CreateHttpClient()
    {
        var h = CreateHttpHandler();
        var client = new HttpClient(h)
        {
            DefaultRequestVersion = HttpVersion.Version20, // 最低使用 HTTP/2
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher, // 仅使用 H2 发送
            Timeout = TimeSpan.FromSeconds(4),
        };
        return client;
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                httpClient?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            httpClient = null;
            cookieContainer = null;
            disposedValue = true;
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
