using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Services.Abstractions;

public interface ICookieHttpClientFactory
{
    CookieContainer GetCookieContainer();

    HttpClient CreateClient();
}
