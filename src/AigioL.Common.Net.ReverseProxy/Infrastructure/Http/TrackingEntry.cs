namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

sealed class TrackingEntry : IDisposable
{
#pragma warning disable IDE0290 // 使用主构造函数
    public TrackingEntry(DelegatingHandler handler)
#pragma warning restore IDE0290 // 使用主构造函数
    {
        disposable = handler.InnerHandler;
        weakReference = new(handler);
    }

    /// <summary>
    /// 用于释放资源的对象
    /// </summary>
    HttpMessageHandler? disposable;

    /// <summary>
    /// 监视对象的弱引用
    /// </summary>
    readonly WeakReference weakReference;

    bool disposedValue;

    /// <summary>
    /// 获取是否可以释放资源
    /// </summary>
    public bool CanDispose => weakReference.IsAlive == false;

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                disposable?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposable = null;
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