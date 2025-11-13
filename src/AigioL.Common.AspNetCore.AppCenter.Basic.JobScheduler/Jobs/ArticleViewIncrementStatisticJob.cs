using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Jobs;

/// <summary>
/// 文章浏览量统计任务
/// </summary>
public sealed partial class ArticleViewIncrementStatisticJob(
    IArticleRepository articleRepository,
    IConnectionMultiplexer redisConnection,
    ILogger<ArticleViewIncrementStatisticJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, ArticleViewIncrementStatisticJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        var dbConnection = redisConnection.GetDatabase(CacheKeys.RedisHashIncrementDb);
        var viewKeys = await dbConnection.HashGetAllAsync(CacheKeys.ArticleViewHashKey);

        // 浏览量
        foreach (var item in viewKeys)
        {
            string? itemName = item.Name;
            if (ShortGuid.TryParse(itemName, out Guid id))
            {
                var viewCount = (long)item.Value;
                if (viewCount > 0)
                {
                    try
                    {
                        var addViewCount = await articleRepository.AddViewCountAsync(id, viewCount);
                        if (addViewCount > 0)
                        {
                            // 减去查询后的次数
                            await dbConnection.HashDecrementAsync(CacheKeys.ArticleViewHashKey, item.Name, viewCount);
                        }
                        else
                        {
                            LogErrorAddViewCount(logger, null, id, viewCount);
                            await dbConnection.HashDeleteAsync(CacheKeys.ArticleViewHashKey, item.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        LogErrorAddViewCount(logger, e, id, viewCount);
                    }
                }
            }
        }

        return true;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "添加文章浏览量出错 Id:{id}，浏览量:{viewCount}")]
    private static partial void LogErrorAddViewCount(ILogger logger, Exception? ex, Guid id, long viewCount);
}
