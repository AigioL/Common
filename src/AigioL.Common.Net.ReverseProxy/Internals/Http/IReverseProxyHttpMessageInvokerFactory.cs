using AigioL.Common.Net.ReverseProxy.Internals.Configuration;

namespace AigioL.Common.Net.ReverseProxy.Internals.Http;

interface IReverseProxyHttpMessageInvokerFactory
{
    ReverseProxyHttpMessageInvoker CreateHttpMessageHandler(string domain, IDomainConfig domainConfig);
}
