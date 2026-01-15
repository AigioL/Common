using System.Net;
using System.Text.Json;

namespace AigioL.Common.Extensions.Http.Proxy.Models;

/// <summary>
/// 包含 <see cref="HttpClient"/> 类的 HTTP 代理设置的可序列化模型类
/// </summary>
public partial record class WebProxyModel
{
    /// <summary>
    /// 获取或设置代理服务器的地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 获取或设置一个地址数组，这些地址不使用代理服务器
    /// </summary>
    public string[]? BypassList { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否跳过代理服务器而使用本地地址
    /// </summary>
    public bool BypassProxyOnLocal { get; set; }

    /// <summary>
    /// 获取或设置要提交到代理服务器进行身份验证的凭据
    /// </summary>
    public CredentialsModel? Credentials { get; set; }

    /// <summary>
    /// 获取或设置 <see cref="bool"/> 值，该值控制 <see cref="CredentialCache.DefaultCredentials "/> 是否随请求一起发送
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    public override string ToString()
    {
        var str = JsonSerializer.Serialize(this, ExHttpProxyJsonSerializerContext.Default.WebProxyModel);
        return str;
    }

    public WebProxy GetWebProxy()
    {
        var credentials = Credentials?.GetCredentials();
        WebProxy p = new(Address, BypassProxyOnLocal, BypassList, credentials)
        {
            UseDefaultCredentials = UseDefaultCredentials,
        };
        return p;
    }
}
