using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using RabbitMQ.Client;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Jobs;

/// <summary>
/// 扣款超时的商家扣款协议推送协议解约申请通知任务
/// </summary>
public class MerchantDeductionAgreementUnSignJob(
    IMerchantDeductionAgreementRepository merchantDeductionAgreementRepository,
    IConnection rabbitmqConn,
    ILogger<MerchantDeductionAgreementUnSignJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) :
    JobService<AppDbContext, MerchantDeductionAgreementUnSignJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        try
        {
            var agreements = await merchantDeductionAgreementRepository.GetMerchantAgreementOfDeductionTimeout();

            foreach (var agreement in agreements)
            {
                await CacheKeys.PushAgreementUnSignRequestMessageAsync(rabbitmqConn, agreement.AgreementNo);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "扣款超时的商家扣款协议推送协议解约申请通知任务异常");
            throw;
        }
    }
}
