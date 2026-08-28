namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Logging;

/// <summary>
/// <see cref="IRequestLoggingFeature"/> 的默认实现类
/// </summary>
sealed class RequestLoggingFeature : IRequestLoggingFeature
{
    /// <summary>
    /// 是否启用，默认值为 <see langword="true"/>
    /// </summary>
    public bool Enable { get; set; } = true;
}