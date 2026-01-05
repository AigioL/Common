using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Jobs;

/// <summary>
/// 订单超时处理任务
/// </summary>
public class OrderTimeoutProcessJob(
    IOrderRepository orderRepository,
    ILogger<OrderTimeoutProcessJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) :
    JobService<AppDbContext, OrderTimeoutProcessJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        try
        {
            await orderRepository.ProcessTimeoutOrderStatus();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "超时未付款的订单设置状态为过期操作异常");
            throw;
        }
    }
}
