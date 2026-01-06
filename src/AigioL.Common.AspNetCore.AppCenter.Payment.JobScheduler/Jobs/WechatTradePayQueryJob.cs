//using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
//using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
//using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
//using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;

//namespace AigioL.Common.AspNetCore.AppCenter.Payment.Jobs;

///// <summary>
///// 微信支付交易状态查询任务
///// </summary>
//public partial class WechatTradePayQueryJob(
//    IWeChatPayServices weChatPayServices,
//    IPaymentRepository paymentRepo,
//    IPaymentService paymentService,
//    ILogger<WechatTradePayQueryJob> logger,
//    AppDbContext dbContext,
//    IFeishuApiClient feishuApiClient) :
//    JobService<AppDbContext, WechatTradePayQueryJob>(logger, dbContext, feishuApiClient)
//{
//    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var paymentList = await paymentRepo.GetPaymentResultWaitingPaymentListAsync(PaymentType.WeChatPay);

//            foreach (OrderPaymentComposition payment in paymentList)
//            {
//                var result = await weChatPayServices.OrderQuery(payment.Order!.Id);
//                switch (result?.TradeStatus)
//                {
//                    // 交易创建，等待买家付款
//                    case TradeStatus.WaitBuyerPay:
//                        // 无操作，继续等下下次查询状态
//                        break;

//                    // 未付款交易超时关闭，或支付完成后全额退款
//                    case TradeStatus.TradeClosed:
//                        await paymentService.NotifyOrderClose(payment.Id);
//                        break;

//                    // 交易支付成功
//                    case TradeStatus.TradeSuccess:
//                        await paymentService.NotifyOrderComplete(payment.Order.Id, result.TransactionId, PaymentType.WeChatPay, result.TotalAmount, result.TimeEnd);
//                        break;

//                    // 交易结束，不可退款
//                    case TradeStatus.TradeFinished:
//                        // 商家签约的产品不支持退款功能的前提下，买家付款成功。
//                        // 或者，商家签约的产品支持退款功能的前提下，交易已经成功并且已经超过可退款期限。
//                        if (payment.PaymentStatus == PaymentStatus.WaitPay)
//                        {
//                            await paymentService.NotifyOrderComplete(payment.Order.Id, result.TransactionId, PaymentType.WeChatPay, result.TotalAmount, result.TimeEnd);
//                        }
//                        // TODO: 看要不要吧，交易完成且已过退款期限
//                        //else if (payment.PaymentStatus == PaymentStatus.Paid)
//                        //    await paymentService.NotifyOrderClose(payment.Id);
//                        break;
//                }
//            }

//            return true;
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "微信支付交易状态查询任务异常");
//            throw;
//        }
//    }
//}
