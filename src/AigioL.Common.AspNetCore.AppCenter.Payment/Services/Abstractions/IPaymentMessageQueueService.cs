using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;

/// <summary>
/// 支付相关的消息队列服务
/// </summary>
public partial interface IPaymentMessageQueueService
{
    /// <summary>
    /// 添加支付成功消息通知
    /// </summary>
    Task PushPaymentSuccess(OrderPaymentSuccessInfo info);

    /// <summary>
    /// 添加退款成功消息通知
    /// </summary>
    Task PushRefundSuccess(OrderRefundSuccessInfo info);

    /// <summary>
    /// 添加签约成功消息通知
    /// </summary>
    Task PushSignAgreementSuccess(string agreementNo);

    /// <summary>
    /// 添加解约成功消息通知
    /// </summary>
    Task PushUnSignAgreementSuccess(string agreementNo);
}
