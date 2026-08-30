using AigioL.Common.Net.ReverseProxy.Models;

namespace AigioL.Common.Net.ReverseProxy;

/// <summary>
/// 反向代理 Web 应用
/// </summary>
public partial interface IReverseProxyWebApp : IAsyncDisposable
{
    /// <summary>
    /// 启动反向代理 Web 应用
    /// </summary>
    Task<ReverseProxyWebAppStartErrCode> StartAsync();

    /// <summary>
    /// 停止反向代理 Web 应用
    /// </summary>
    Task StopAsync();
}
