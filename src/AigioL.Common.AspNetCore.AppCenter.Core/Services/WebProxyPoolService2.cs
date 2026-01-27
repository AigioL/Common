using AigioL.Common.AspNetCore.AppCenter.Repositories.Net.Abstractions;
using AigioL.Common.Extensions.Http.Proxy.Models;
using AigioL.Common.Extensions.Http.Proxy.Services;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Services;

public partial class WebProxyPoolService2(
    IConnectionMultiplexer connection,
    IWebProxyRepository repo) :
    WebProxyPoolService(connection)
{
    protected override Task<WebProxyModel[]> GetWebProxiesAsync(CancellationToken cancellationToken = default)
    {
        var r = repo.GetWebProxiesAsync(cancellationToken: cancellationToken);
        return r;
    }
}
