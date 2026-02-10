using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Jobs;

/// <summary>
/// 商家协议扣款任务（需要由业务重写 CreateOrderAsync）
/// </summary>
public abstract partial class MerchantDeductionJob(
    IAliPayServices aliPayService,
    IWeChatPayServices weChatPayServices,
    IMerchantDeductionAgreementRepository agreementRepo,
    IKeyValuePairRepository keyValuePairRepo,
    ILogger<MerchantDeductionJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) :
    JobService<AppDbContext, MerchantDeductionJob>(logger, dbContext, feishuApiClient)
{
    /// <summary>
    /// 如果向支付平台要求扣款出错了（通常是网络有问题），可重试 3 次
    /// </summary>
    public virtual AsyncRetryPolicy RetryPolicy
    {
        get
        {
            field ??= GetRetryPolicy();
            return field;
        }
    }

    protected virtual AsyncRetryPolicy GetRetryPolicy() => Policy.Handle<Exception>().RetryAsync(3);

    /// <summary>
    /// 如果连续 10 个扣款协议失败，则直接结束本次 Job
    /// <para>失败的扣款协议虽然仍是签约状态，但由于已产生订单，程序将不会继续向支付平台要求扣款</para>
    /// </summary>
    protected virtual AsyncCircuitBreakerPolicy CircuitBreakerPolicy
    {
        get
        {
            field ??= GetCircuitBreakerPolicy();
            return field;
        }
    }

    protected virtual AsyncCircuitBreakerPolicy GetCircuitBreakerPolicy() =>
        Policy.Handle<Exception>().CircuitBreakerAsync(10, TimeSpan.FromMinutes(1));

    const int maxFailedSendMessageCount = 3;

    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        try
        {
            var daysInAdvanceString = await keyValuePairRepo.QueryValueAsync(CacheKeys.商家扣款提前天数, cancellationToken);
            int daysInAdvance = int.TryParse(daysInAdvanceString, out var days) ? days : CacheKeys.商家扣款提前天数默认值;

            int failedCount = 0;
            var agreements = await agreementRepo.GetAgreementsForDeduction(daysInAdvance, cancellationToken: cancellationToken);
            foreach (var agreement in agreements)
            {
                try
                {
                    await CircuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        // 创建订单
                        var order = await CreateOrderAsync(agreement);
                        if (order != null)
                        {
                            // 执行扣款（支持重试）
                            await RetryPolicy.ExecuteAsync(() =>
                                ExecuteAgreementDeductionAsync(agreement, order));
                        }
                    });
                }
                catch (BrokenCircuitException ex)
                {
                    throw new ApplicationException($"连续 N 个协议执行扣款失败，异常结束", ex);
                }
                catch (Exception ex)
                {
                    LogError(logger, ex, agreement.AgreementNo);
                    failedCount++;

                    if (failedCount <= maxFailedSendMessageCount)
                    {
                        // 避免频繁发送通知，最多发送 3 次失败通知
                        var title = $"JobErr: {JobName}";
                        var meesage = $"执行商家协议扣款失败：{agreement.AgreementNo}，失败次数：{failedCount}，失败最大通知次数：{maxFailedSendMessageCount}";
                        await feishuApiClient.SendMessageAsync(title, meesage, CancellationToken.None); // 飞书通知不取消
                    }
                }
                await Task.Delay(10, cancellationToken); // 防止调用频率过快
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "商家协议扣款任务异常");
            throw;
        }
    }

    /// <summary>
    /// 由业务重写此方法创建通用订单与业务订单并关联
    /// </summary>
    /// <param name="agreement"></param>
    /// <returns></returns>
    protected abstract Task<Order?> CreateOrderAsync(MerchantDeductionAgreement agreement);

    protected virtual async Task ExecuteAgreementDeductionAsync(MerchantDeductionAgreement agreement, Order order)
    {
        switch (agreement.Platform)
        {
            case PaymentType.Alipay:
                await aliPayService.ExecuteAgreementDeduction(
                    order.Id,
                    order.Note ?? string.Empty,
                    agreement.SingleAmount,
                    agreement.ExtAgreementNo);
                return;
            case PaymentType.WeChatPay:
                await weChatPayServices.ExecuteAgreementDeduction(
                    order.Id,
                    order.Note ?? string.Empty,
                    agreement.SingleAmount,
                    agreement.ExtAgreementNo);
                return;
            default:
                // 重写此函数在调用 base 之前处理其他支付平台类型则不会抛出异常
                throw new ArgumentOutOfRangeException(nameof(agreement.Platform), agreement.Platform, null);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "执行商家协议扣款失败：{agreementNo}")]
    private static partial void LogError(ILogger logger, Exception? ex, string? agreementNo);
}
