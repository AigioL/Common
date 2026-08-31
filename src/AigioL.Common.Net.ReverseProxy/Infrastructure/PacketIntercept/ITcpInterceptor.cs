namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

/// <summary>
/// TCP 数据包拦截器接口
/// </summary>
interface ITcpInterceptor
{
    /// <summary>
    /// 拦截 TCP 数据包
    /// </summary>
    Task InterceptAsync(CancellationToken cancellationToken);
}