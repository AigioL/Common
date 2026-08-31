namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

/// <summary>
/// DNS 数据包拦截器接口
/// </summary>
interface IDnsInterceptor
{
    /// <summary>
    /// 拦截 DNS 数据包
    /// </summary>
    Task InterceptAsync(CancellationToken cancellationToken);
}
