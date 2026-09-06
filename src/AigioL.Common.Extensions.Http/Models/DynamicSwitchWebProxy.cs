using System.Net;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

/// <summary>
/// 可动态切换设置内部 <see cref="IWebProxy"/> 实例的 <see cref="WebProxy"/> 实现
/// <para>将此类型创建实例赋值给 <see cref="SocketsHttpHandler.Proxy"/> 可使用 <see cref="SetWebProxy(IWebProxy?)"/> 在运行中更换代理</para>
/// </summary>
public sealed partial class DynamicSwitchWebProxy : IWebProxy, IIsNoProxy
{
    static readonly AsyncLocal<IWebProxy> asyncLocal = new();

    /// <summary>
    /// 当未使用 <see cref="SetWebProxy(IWebProxy?)"/> 设置要切换的代理时，使用的默认代理，默认值将会是 <see cref="HttpClient.DefaultProxy"/>
    /// </summary>
    public static IWebProxy DefaultProxy
    {
        get => field ?? HttpClient.DefaultProxy;
        set => field = value;
    }

    public ICredentials? Credentials
    {
        get
        {
            var v = asyncLocal.Value;
            if (v == null)
                return DefaultProxy.Credentials;
            return v.Credentials;
        }
        set
        {
            // 在运行时更换代理时，Credentials 也会随之更换，不支持在切换的实现上设置值
            throw new NotSupportedException();
        }
    }

    public Uri? GetProxy(Uri destination)
    {
        var v = asyncLocal.Value;
        if (v == null)
            return DefaultProxy.GetProxy(destination);
        return v.GetProxy(destination);
    }

    public bool IsBypassed(Uri host)
    {
        var v = asyncLocal.Value;
        if (v == null)
            return DefaultProxy.IsBypassed(host);
        return v.IsBypassed(host);
    }

    public bool IsNoProxy()
    {
        var v = asyncLocal.Value;
        if (v == null)
            return HttpNoProxy.IsNoProxy(DefaultProxy);
        return HttpNoProxy.IsNoProxy(v);
    }

    public void SetWebProxy(IWebProxy? webProxy = null)
    {
        asyncLocal.Value = webProxy ?? HttpNoProxy.Instance;
    }

    public void SetWebProxy(SocketsHttpHandler handler)
    {
        handler.Proxy = this;
        handler.UseProxy = true;
    }

    public void SetWebProxy(HttpClientHandler handler)
    {
        handler.Proxy = this;
        handler.UseProxy = true;
    }
}
