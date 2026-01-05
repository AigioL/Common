#pragma warning disable IDE0290 // 使用主构造函数
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Workers;

public sealed partial class PaymentRefundSubscribe : WorkerBackgroundService
{
    readonly IServiceProvider serviceProvider;

    public PaymentRefundSubscribe(
        ILogger<PaymentRefundSubscribe> logger,
        IServiceProvider serviceProvider,
        IOptions<JsonOptions> jsonOptions,
        IConnection rabbitmqConn,
        IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override string RoutingKey => CacheKeys.OrderCompleted;

    protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        string? orderNumber = null;
        string? refundNumber = null;
        try
        {
            using var container = serviceProvider.CreateScope();
            var paymentRepo = container.ServiceProvider.GetRequiredService<IPaymentRepository>();
            var paymentMessageQueue = container.ServiceProvider.GetRequiredService<IPaymentMessageQueueService>();

            var orderRefundMessage = JsonSerializer.Deserialize(eventArgs.Body.Span, PaymentMinimalApisJsonSerializerContext.Default.OrderRefundMessage);
            ArgumentNullException.ThrowIfNull(orderRefundMessage);
            orderNumber = orderRefundMessage.OrderNumber;
            ArgumentNullException.ThrowIfNull(orderNumber);
            refundNumber = orderRefundMessage.RefundNumber;
            ArgumentNullException.ThrowIfNull(refundNumber);

            var paymentComposition = await paymentRepo.GetOnlinePaidCompositionAsync(orderNumber)
               ?? throw new ApplicationException("未找到已支付的在线支付组成");
            (var order, _) = await paymentRepo.GetOrderPaymentAsync(orderNumber, paymentComposition.PaymentType, cancellationToken: cancellationToken)
               ?? throw new ApplicationException("未找到订单信息");
            var refundBill = await paymentRepo.GetRefundBillAsync(refundNumber, cancellationToken: cancellationToken)
                ?? throw new ApplicationException("未找到退款单");
            if (refundBill.RefundAmount > paymentComposition.PaymentAmount)
                throw new ApplicationException("退款金额不能大于支付金额");

            (bool success, bool refundSuccess, string code, string errorDesc) result;
            switch (paymentComposition.PaymentType)
            {
                case PaymentType.Alipay:
                    var aliPayServices = container.ServiceProvider.GetRequiredService<IAliPayServices>();
                    result = await aliPayServices.Refund(order.Id, refundBill.RefundNumber, refundBill.RefundAmount);
                    break;
                case PaymentType.WeChatPay:
                    // TODO: 微信支付v2版接口只能从查询接口中得到退款结果
                    //       (申请退款接口的返回仅代表业务的受理情况，具体退款是否成功，需要通过退款查询接口获取结果。)
                    var weChatPayServices = container.ServiceProvider.GetRequiredService<IWeChatPayServices>();
                    result = await weChatPayServices.Refund(order.Id, refundBill.RefundNumber, refundBill.RefundAmount, paymentComposition.PaymentAmount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(paymentComposition.PaymentType), paymentComposition.PaymentType, null);
            }

            if (result.success && result.refundSuccess) // 申请退款并直接退款成功
            {
                var refundInfo = new OrderRefundSuccessInfo(order.Id, refundBill.RefundNumber, paymentComposition.PaymentType);
                await paymentRepo.CompleteRefundForOrderAsync(refundInfo);    // 完成订单退款
                await paymentMessageQueue.PushRefundSuccess(refundInfo); // 推送退款完成的消息
            }
            else
            {
                await paymentRepo.UpdateRefundBillAsync(refundNumber, result.success, result.refundSuccess, $"{result.code} {result.errorDesc}");
            }

            return true;
        }
        catch (Exception ex)
        {
#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
            LogErrorHandle(logger, ex, orderNumber, refundNumber, Encoding.UTF8.GetString(eventArgs.Body.Span));
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
            throw;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "申请退款处理错误，订单号：{orderNumber}，refundNumber：{refundNumber}，消息内容：{message}")]
    private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? orderNumber, string? refundNumber, string? message);
}
