namespace AigioL.Common.Net.ReverseProxy.Internals.Logging;

/// <summary>
/// 反向代理服务请求日志功能
/// </summary>
interface IRequestLoggingFeature
{
    /// <summary>
    /// 是否启用
    /// </summary>
    bool Enable { get; set; }
}
