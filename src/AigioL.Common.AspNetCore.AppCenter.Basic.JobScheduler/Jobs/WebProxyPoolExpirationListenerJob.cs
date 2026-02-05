using AigioL.Common.Extensions.Http.Proxy.Services.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Jobs;

/// <summary>
/// <see cref="IWebProxyPoolService"/> 过期代理监听原子清理和计数递减任务
/// </summary>
public sealed partial class WebProxyPoolExpirationListenerJob(
    IWebProxyPoolService webProxyPool,
    ILogger<WebProxyPoolExpirationListenerJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, WebProxyPoolExpirationListenerJob>(logger, dbContext, feishuApiClient)
{
    bool isFirst = true;

    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        if (isFirst)
        {
            // 首次启动时将数据从数据库加载到 Redis 中
            await webProxyPool.UpdateWebProxiesToCacheAsync(cancellationToken);
        }

        await webProxyPool.CleanupExpiredProxiesAsync();

        isFirst = false;
        return true;
    }
}
