#pragma warning disable IDE0290 // 使用主构造函数
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Workers;

/// <summary>
/// 支付结果订阅
/// </summary>
public static partial class PaymentResultSubscribe
{
    const string exchangeName = ""; // 默认交换机

    internal static async Task ListRightPushAsync(
        IConnection rabbitmqConn,
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        using var channel = await rabbitmqConn.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicPublishAsync(exchangeName, routingKey, body, cancellationToken);
    }

    /// <summary>
    /// 处理【付款完成】的订单
    /// </summary>
    public sealed partial class OrderPaymentSuccessWorker : WorkerBackgroundService
    {
        readonly IServiceProvider serviceProvider;

        public OrderPaymentSuccessWorker(
            ILogger<OrderPaymentSuccessWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<JsonOptions> jsonOptions,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override string RoutingKey => CacheKeys.OrderPaymentSuccess;

        protected override string QueueName => CacheKeys.OrderQueueName;

        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            string? orderNumber = null;
            try
            {
                using var container = serviceProvider.CreateScope();
                var orderRepo = container.ServiceProvider.GetRequiredService<IOrderRepository>();

                var paymentInfo = JsonSerializer.Deserialize(eventArgs.Body.Span, PaymentMinimalApisJsonSerializerContext.Default.OrderPaymentSuccessInfo);
                ArgumentNullException.ThrowIfNull(paymentInfo);
                orderNumber = paymentInfo.OrderNumber;

                var businessTypeId = await orderRepo.GetBusinessTypeIdByOrderNumberAsync(paymentInfo.OrderNumber, cancellationToken);
                if (!businessTypeId.HasValue)
                {
                    throw new ApplicationException("找不到订单");
                }

                // 传递付款完成通知给业务订单
                var routingKey = CacheKeys.GetPaymentSuccessMessageQueueKeyByBusinessType(businessTypeId.Value);
                await ListRightPushAsync(rabbitmqConn, routingKey, Encoding.UTF8.GetBytes(orderNumber), CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
                LogErrorHandle(logger, ex, orderNumber, Encoding.UTF8.GetString(eventArgs.Body.Span));
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
                throw;
            }
        }

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "订单支付成功下发业务失败，订单号：{orderNumber}，消息内容：{message}")]
        private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? orderNumber, string? message);
    }

    /// <summary>
    /// 处理【退款完成】的订单
    /// </summary>
    public sealed partial class OrderRefundSuccessWorker : WorkerBackgroundService
    {
        readonly IServiceProvider serviceProvider;

        public OrderRefundSuccessWorker(
            ILogger<OrderRefundSuccessWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<JsonOptions> jsonOptions,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override string RoutingKey => CacheKeys.OrderRefundSuccess;

        protected override string QueueName => CacheKeys.OrderQueueName;

        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            string? orderNumber = null;
            try
            {
                using var container = serviceProvider.CreateScope();
                var orderRepo = container.ServiceProvider.GetRequiredService<IOrderRepository>();

                var orderRefundInfo = JsonSerializer.Deserialize(eventArgs.Body.Span, PaymentMinimalApisJsonSerializerContext.Default.OrderRefundSuccessInfo);
                ArgumentNullException.ThrowIfNull(orderRefundInfo);
                orderNumber = orderRefundInfo.OrderNumber;

                //传递退款完成通知给业务订单
                var businessTypeId = await orderRepo.GetBusinessTypeIdByOrderNumberAsync(orderRefundInfo.OrderNumber, cancellationToken);
                if (!businessTypeId.HasValue)
                {
                    throw new ApplicationException("找不到订单");
                }
                var routingKey = CacheKeys.GetOrderRefundedMessageQueueKeyByBusinessType(businessTypeId.Value);
                await ListRightPushAsync(rabbitmqConn, routingKey, Encoding.UTF8.GetBytes(orderNumber), CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
                LogErrorHandle(logger, ex, orderNumber, Encoding.UTF8.GetString(eventArgs.Body.Span));
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
                throw;
            }
        }

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "订单退款成功下发业务失败，订单号：{orderNumber}，消息内容：{message}")]
        private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? orderNumber, string? message);
    }
}
