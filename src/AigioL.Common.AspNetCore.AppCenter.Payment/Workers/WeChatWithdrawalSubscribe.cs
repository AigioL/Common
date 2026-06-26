using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Workers;

/// <summary>
/// 微信提现订阅 Worker
/// </summary>
/// <remarks>
/// TODO: 后续开发微信提现完整逻辑
/// 当前为占位实现，仅定义 Worker 框架和处理入口
/// </remarks>
public partial class WeChatWithdrawalSubscribe : WorkerBackgroundService
{
    readonly IServiceProvider serviceProvider;

    public WeChatWithdrawalSubscribe(
        ILogger<WeChatWithdrawalSubscribe> logger,
        IServiceProvider serviceProvider,
        IOptions<JsonOptions> jsonOptions,
        IConnection rabbitmqConn,
        IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    protected override string RoutingKey => "PCUserWeChatWithdrawalRequest";

    /// <inheritdoc/>
    protected override string QueueName => $"{CacheKeys.OrderQueueName}.PCUserWeChatWithdrawalRequest";

    /// <inheritdoc/>
    protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        try
        {
            var body = Encoding.UTF8.GetString(eventArgs.Body.Span);
            logger.LogInformation("收到微信提现请求消息：{Body}", body);

            // TODO: 实现微信提现完整逻辑
            // 1. 解析消息体获取提现单号
            // 2. 查询 PCUserWithdrawalRecord 获取提现详情
            // 3. 调用微信支付转账接口（商家转账到零钱）
            // 4. 根据微信返回结果更新提现记录状态
            // 5. 如果失败则退回金额到钱包可提现金额
            // 6. 创建对应的 PCUserWalletChangeRecord

            // 当前占位：直接返回成功
            await Task.CompletedTask;

            logger.LogInformation("微信提现处理完成（占位）");
        }
        catch (Exception ex)
        {
            var workerName = WorkerName;
            var body = Encoding.UTF8.GetString(eventArgs.Body.Span);
            var message = $"处理微信提现订阅方法执行异常，消息内容：{body}";
            await OnHandleFailAsync(workerName, message);
            LogErrorHandle(logger, ex, body);
        }
        return true;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "处理微信提现订阅方法执行异常，消息内容：{body}")]
    private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? body);
}
