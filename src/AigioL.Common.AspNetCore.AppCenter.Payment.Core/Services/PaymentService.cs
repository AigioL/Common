using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

sealed partial class PaymentService(
    ILogger<PaymentService> logger,
    IPaymentMessageQueueService paymentMessageQueue,
    IPaymentRepository paymentRepo,
    IMerchantDeductionAgreementRepository agreementRepo) : IPaymentService
{
    public async Task NotifyOrderClose(Guid paymentId)
    {
        await paymentRepo.ClosePayment(paymentId);
    }

    public async Task NotifyOrderComplete(string orderNumber, string tradeNo, PaymentType paymentType, decimal amountReceived, DateTimeOffset paymentTime)
    {
        var orderInfo = await paymentRepo.GetOrderPaymentAsync(orderNumber, paymentType);
        if (orderInfo is not var (order, paymentComposition))
        {
            logger.LogError("平台支付完成回调通知，找不到订单信息");
            return;
        }

        //if (order.Status == OrderStatus.Paid && paymentComposition.PaymentStatus == PaymentStatus.Paid)
        if (order.Status is not (OrderStatus.WaitPay or OrderStatus.Expired))
        {
            logger.LogInformation("平台支付完成回调通知，出现重复调用的情况：" +
                "OrderNumber: {OrderNumber} TradeNo: {TradeNo} PaymentType: {PaymentType} AmountReceived: {AmountReceived} PaymentTime: {PaymentTime}",
                orderNumber, tradeNo, paymentType, amountReceived, paymentTime);
            return;
        }

        var info = new OrderPaymentSuccessInfo(paymentComposition.Id, order.Id, paymentType, tradeNo, amountReceived, paymentTime);

        // 完成订单支付并推送“支付完成”的消息
        await paymentRepo.CompletePaymentForOrder(info);
        await paymentMessageQueue.PushPaymentSuccess(info);

        // 如果是按商家扣款协议创建的订单，则更新下次扣款时间
        if (order.MerchantDeductionAgreement is MerchantDeductionAgreement agreement)
        {
            await agreementRepo.UpdateNextDeductionTime(agreement, paymentTime);
        }
    }
}
