using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

interface IReverseProxyHttpMessageInvokerFactory
{
    ReverseProxyHttpMessageInvoker CreateHttpMessageHandler(string domain, IDomainConfig domainConfig);
}
