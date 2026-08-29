using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;
using System.Collections.Concurrent;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

sealed class ReverseProxyHttpMessageInvokerFactory : IReverseProxyHttpMessageInvokerFactory
{
    readonly IDomainResolver domainResolver;

    readonly IReverseProxyConfig reverseProxyConfig;

    /// <summary>
    /// 首次生命周期
    /// </summary>
    readonly TimeSpan firstLiftTime = TimeSpan.FromSeconds(10d);

    /// <summary>
    /// 非首次生命周期
    /// </summary>
    readonly TimeSpan nextLifeTime = TimeSpan.FromSeconds(100d);

    readonly LifetimeHttpHandlerCleaner cleaner = new();

    readonly ConcurrentDictionary<LifeTimeKey, Lazy<LifetimeHttpHandler>> cache = new();

#pragma warning disable IDE0290 // 使用主构造函数
    public ReverseProxyHttpMessageInvokerFactory(IDomainResolver domainResolver, IReverseProxyConfig reverseProxyConfig)
#pragma warning restore IDE0290 // 使用主构造函数
    {
        this.domainResolver = domainResolver;
        this.reverseProxyConfig = reverseProxyConfig;
    }


    public ReverseProxyHttpMessageInvoker CreateHttpMessageHandler(string domain, IDomainConfig domainConfig)
    {
        var lifeTimeKey = new LifeTimeKey { Domain = domain, DomainConfig = domainConfig, };
        var lifetimeHttpHandler = cache.GetOrAdd(lifeTimeKey, CreateFirstLifetimeHttpHandlerLazy).Value;
        var r = new ReverseProxyHttpMessageInvoker(lifetimeHttpHandler, false, reverseProxyConfig);
        return r;
    }

    Lazy<LifetimeHttpHandler> CreateFirstLifetimeHttpHandlerLazy(LifeTimeKey lifeTimeKey)
        => new(() =>
        {
            var h = CreateLifetimeHttpHandler(lifeTimeKey, firstLiftTime);
            return h;
        }, true);

    LifetimeHttpHandler CreateLifetimeHttpHandler(LifeTimeKey lifeTimeKey, TimeSpan lifeTime)
    {
        LifetimeHttpHandler h = new(domainResolver, reverseProxyConfig, lifeTimeKey, lifeTime, OnLifetimeHttpHandlerDeactivate);
        return h;
    }

    void OnLifetimeHttpHandlerDeactivate(LifetimeHttpHandler lifetimeHttpHandler)
    {
        var lifeTimeKey = lifetimeHttpHandler.LifeTimeKey;
        cache[lifeTimeKey] = CreateNextLifetimeHttpHandlerLazy(lifeTimeKey);
        cleaner.Add(lifetimeHttpHandler);
    }

    Lazy<LifetimeHttpHandler> CreateNextLifetimeHttpHandlerLazy(LifeTimeKey lifeTimeKey)
        => new(() =>
        {
            var h = CreateLifetimeHttpHandler(lifeTimeKey, nextLifeTime);
            return h;
        }, true);
}
