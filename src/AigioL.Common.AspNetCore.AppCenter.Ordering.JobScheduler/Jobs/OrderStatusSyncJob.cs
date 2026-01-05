//using StackExchange.Redis;

//namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Jobs;

///// <summary>
///// 订单状态同步任务
///// </summary>
//public sealed partial class OrderStatusSyncJob(
//    IConnectionMultiplexer redisConnection,
//    ILogger<OrderStatusSyncJob> logger,
//    AppDbContext dbContext,
//    IFeishuApiClient feishuApiClient) :
//    JobService<AppDbContext, OrderStatusSyncJob>(logger, dbContext, feishuApiClient)
//{
//    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
//    {
//        // 核心逻辑就是从数据库中查找出通用订单状态与业务订单不同状态不同的订单Id
//        // 在这些未同步的订单 Id 中再排除掉 Redis 中存在的（未被消费掉，所以两边状态不同步是正常的），结果就是丢失的订单 Id
//        try
//        {
//            return true;
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "通用订单与业务订单同步失败");
//            throw;
//        }
//    }
//}
