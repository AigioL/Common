using AigioL.Common.Extensions.Http.Proxy.Services.Abstractions;
using AigioL.Common.Models;
using System.Diagnostics;
using System.Net;

namespace AigioL.Common.Extensions.Http.Proxy.Services;

partial class WebProxyPoolService : IWebProxyPoolService
{
    protected string ConnectionTestUrl => "http://www.msftconnecttest.com/connecttest.txt";

    public async Task<ApiRsp<TimeSpan?>> ConnectionTestAsync(
        IWebProxy webProxy,
        CancellationToken cancellationToken = default)
    {
        using var h = new SocketsHttpHandler
        {
            UseProxy = true,
            Proxy = webProxy,
            AllowAutoRedirect = false,
        };
        using var c = new HttpClient(h);
        using var req = new HttpRequestMessage(HttpMethod.Get, ConnectionTestUrl);

        var sw = Stopwatch.StartNew();

        using var rsp = await c.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        sw.Stop();

        return sw.Elapsed;
    }
}
