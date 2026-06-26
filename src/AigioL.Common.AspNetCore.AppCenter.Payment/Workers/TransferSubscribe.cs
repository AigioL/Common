using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
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

/// <summary>
/// 转账订阅
/// </summary>
public partial class TransferSubscribe : WorkerBackgroundService
{
    readonly IServiceProvider serviceProvider;

    public TransferSubscribe(
        ILogger<TransferSubscribe> logger,
        IServiceProvider serviceProvider,
        IOptions<JsonOptions> jsonOptions,
        IConnection rabbitmqConn,
        IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override string RoutingKey => CacheKeys.TransferRequest;

    protected override string QueueName => $"{CacheKeys.OrderQueueName}.{RoutingKey}";

    protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            // 获取需要的服务
            var transferOrderRepo = scope.ServiceProvider.GetRequiredService<ITransferOrderRepository>();
            var aliPayService = scope.ServiceProvider.GetRequiredService<IAliPayServices>();
            var weChatPayService = scope.ServiceProvider.GetRequiredService<IWeChatPayServices>();

            var model = JsonSerializer.Deserialize(eventArgs.Body.Span, PaymentMinimalApisJsonSerializerContext.Default.WithdrawTransferModel)
                ?? throw new ArgumentException("处理转账 msg 参数错误");
            var transferOrder = new TransferOrder()
            {
                UserId = model.UserId,
                TransferNumber = model.TransferNumber,
                Title = model.Title,
                TransferAmount = model.TransferAmount,
                UserOpenId = model.UserOpenId,
                UserLoginAccount = model.UserLoginAccount,
                PaymentPlatform = model.PaymentPlatform,
            };
            await transferOrderRepo.AddTransferOrder(transferOrder);

            // 调用支付平台转账接口
            PubTransferState result;
            switch (model.PaymentPlatform)
            {
                case PaymentType.Alipay:
                    result = await aliPayService.Transfer(transferOrder.TransferNumber, transferOrder.TransferAmount, transferOrder.Title, transferOrder.UserOpenId);
                    break;
                case PaymentType.WeChatPay:
                    result = await weChatPayService.Transfer(transferOrder.TransferNumber, transferOrder.TransferAmount, transferOrder.Title, transferOrder.UserOpenId);
                    break;
                default:
                    throw new ApplicationException("不支持的支付平台");
            }

            // 更新转账单结果
            transferOrder.FailureReason = result.Message;
            transferOrder.ThirdPartyPlatformNumber = result.ThirdPartyPlatformNumber;
            transferOrder.TransferStatus = result.TransferStatus;
            transferOrder.FinishTime = result.FinishTime;
            await transferOrderRepo.UpdateTransferOrderResult(transferOrder);
        }
        catch (Exception ex)
        {
            var workerName = WorkerName;
            var body = Encoding.UTF8.GetString(eventArgs.Body.Span);
            var message = $"处理转账订阅方法，执行异常，消息内容：{body}";
            await OnHandleFailAsync(workerName, message);
            LogErrorHandle(logger, ex, body);
        }
        return true;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "处理转账订阅方法，执行异常，消息内容：{body}")]
    private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? body);
}
