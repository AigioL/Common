using System.Net;

namespace AigioL.Common.Extensions.Http.Proxy.Models;

/// <summary>
/// 可动态设置内部 <see cref="IWebProxy"/> 实例的 <see cref="WebProxy"/> 实现
/// <para>将此类型创建实例赋值给 <see cref="SocketsHttpHandler.Proxy"/> 可使用 <see cref="SetWebProxy(IWebProxy?)"/> 在运行中更换代理</para>
/// </summary>
public sealed partial class DynamicWebProxy : IWebProxy
{
    IWebProxy webProxy;

    public DynamicWebProxy(IWebProxy? webProxy)
    {
        this.webProxy = webProxy ?? HttpNoProxy.Instance;
    }

    public ICredentials? Credentials
    {
        get => webProxy.Credentials;
        set => webProxy.Credentials = value;
    }

    public Uri? GetProxy(Uri destination) => webProxy.GetProxy(destination);

    public bool IsBypassed(Uri host) => webProxy.IsBypassed(host);

    public void SetWebProxy(IWebProxy? webProxy = null)
    {
        this.webProxy = webProxy ?? HttpNoProxy.Instance;
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
