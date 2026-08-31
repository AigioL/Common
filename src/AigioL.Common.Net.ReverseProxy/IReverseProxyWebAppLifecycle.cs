namespace AigioL.Common.Net.ReverseProxy;

/// <summary>
/// 反向代理 Web 应用生命周期接口
/// </summary>
interface IReverseProxyWebAppLifecycle
{
    /// <summary>
    /// 停止反向代理 Web 应用
    /// </summary>
    void Stop();
}
