using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Services.Abstractions;

public interface ICookieHttpClientFactory : IHttpClientFactory
{
    CookieContainer GetCookieContainer();
}
