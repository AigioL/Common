using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using Timer = System.Threading.Timer;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// 具有生命周期的 <see cref="DelegatingHandler"/>
/// </summary>
sealed class LifetimeHttpHandler : DelegatingHandler
{
    readonly Timer timer;

    public LifeTimeKey LifeTimeKey { get; }

    public LifetimeHttpHandler(
        IDnsResolver dnsResolver,
        IReverseProxyConfig reverseProxyConfig,
        LifeTimeKey lifeTimeKey,
        TimeSpan lifeTime,
        Action<LifetimeHttpHandler> deactivateAction)
    {
        LifeTimeKey = lifeTimeKey;
        InnerHandler = new ReverseProxyHttpDelegatingHandler(lifeTimeKey.DomainConfig, dnsResolver, reverseProxyConfig);
        timer = new Timer(OnTimerCallback, deactivateAction, lifeTime, Timeout.InfiniteTimeSpan);
    }

    void OnTimerCallback(object? state)
    {
        timer.Dispose();
        ((Action<LifetimeHttpHandler>)state!)(this);
    }

    protected sealed override void Dispose(bool disposing)
    {
        // 这里不释放资源
    }
}
