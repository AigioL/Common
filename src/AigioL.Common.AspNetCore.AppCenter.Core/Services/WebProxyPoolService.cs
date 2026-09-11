using AigioL.Common.AspNetCore.AppCenter.Repositories.Net.Abstractions;
using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Extensions.Http.ProxyPool.Services.Abstractions;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Services;

public sealed partial class WebProxyPoolService(
    IConnectionMultiplexer connection,
    IServiceProvider serviceProvider) :
    RedisWebProxyPoolServiceBase(connection)
{
    protected override Task<WebProxyModel[]> GetWebProxiesAsync(CancellationToken cancellationToken = default)
    {
        var repo = serviceProvider.GetRequiredService<IWebProxyRepository>();
        var r = repo.GetWebProxiesAsync(cancellationToken: cancellationToken);
        return r;
    }
}
