using System.Net;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

/// <summary>
/// 可动态切换设置内部 <see cref="IWebProxy"/> 实例的 <see cref="WebProxy"/> 实现
/// </summary>
public sealed partial class DynamicSwitchWebProxy : IWebProxy
{
    /// <inheritdoc cref="DynamicSwitchWebProxy"/>
    public static readonly DynamicSwitchWebProxy Instance = new();

    static readonly AsyncLocal<IWebProxy> asyncLocal = new();

    public ICredentials? Credentials
    {
        get
        {
            if (asyncLocal.Value == null)
                return HttpNoProxy.Instance.Credentials;
            return asyncLocal.Value.Credentials;
        }
        set
        {
        }
    }

    public Uri? GetProxy(Uri destination)
    {
        if (asyncLocal.Value == null)
            return HttpNoProxy.Instance.GetProxy(destination);
        return asyncLocal.Value.GetProxy(destination);
    }

    public bool IsBypassed(Uri host)
    {
        if (asyncLocal.Value == null)
            return HttpNoProxy.Instance.IsBypassed(host);
        return asyncLocal.Value.IsBypassed(host);
    }

    public void SetWebProxy(IWebProxy? webProxy = null)
    {
        asyncLocal.Value = webProxy ?? HttpNoProxy.Instance;
    }
}
