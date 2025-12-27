#pragma warning disable IDE0290 // 使用主构造函数
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Workers;

/// <summary>
/// 订单状态订阅
/// </summary>
public static partial class OrderStatusSubscribe
{
    public sealed partial class OrderCompleteWorker : WorkerBackgroundService
    {
        readonly IServiceProvider serviceProvider;

        public OrderCompleteWorker(
            ILogger<OrderCompleteWorker> logger,
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
            try
            {
                using var container = serviceProvider.CreateScope();
                var orderRepo = container.ServiceProvider.GetRequiredService<IOrderRepository>();

                orderNumber = Encoding.UTF8.GetString(eventArgs.Body.Span);
                await orderRepo.CompleteOrderAsync(orderNumber);
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
            Message = "订单支付成功处理错误，订单号：{orderNumber}，消息内容：{message}")]
        private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? orderNumber, string? message);
    }
}
