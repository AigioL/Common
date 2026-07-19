using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using GameTrainer.ApiService.Data;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Workers;

/// <summary>
/// PC 用户微信提现完成回调订阅 Worker
/// <para>消费微信支付转账回调通知，更新提现记录状态并处理退款</para>
/// </summary>
public partial class PCUserWithdrawalCompletedSubscribe : WorkerBackgroundService
{
    readonly IServiceProvider serviceProvider;

    public PCUserWithdrawalCompletedSubscribe(
        ILogger<PCUserWithdrawalCompletedSubscribe> logger,
        IServiceProvider serviceProvider,
        IOptions<JsonOptions> jsonOptions,
        IConnection rabbitmqConn,
        IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    protected override string RoutingKey => CacheKeys.TransferCompleted;

    /// <inheritdoc/>
    protected override string QueueName => $"{CacheKeys.OrderQueueName}.{RoutingKey}";

    /// <inheritdoc/>
    protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        try
        {
            var info = JsonSerializer.Deserialize(eventArgs.Body.Span,
                PaymentMinimalApisJsonSerializerContext.Default.WithdrawalTransferCompletedInfo);

            if (info == null)
            {
                logger.LogWarning("PCUserWithdrawalCompletedSubscribe 收到空消息");
                return true;
            }

            logger.LogInformation(
                "处理微信提现完成回调：OutBillNo={OutBillNo}, TransferBillNo={TransferBillNo}, State={State}",
                info.OutBillNo, info.TransferBillNo, info.State);

            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 查询提现记录
            var record = await db.PCUserWithdrawalRecords
                .FirstOrDefaultAsync(r => r.WithdrawalNumber == info.OutBillNo, cancellationToken);

            if (record == null)
            {
                logger.LogWarning("未找到提现记录：WithdrawalNumber={WithdrawalNumber}", info.OutBillNo);
                return true;
            }

            // 避免重复处理终态
            if (record.Status is PCUserWithdrawalStatus.Success or PCUserWithdrawalStatus.Failed)
            {
                logger.LogInformation(
                    "提现记录已处于终态，跳过处理：WithdrawalNumber={WithdrawalNumber}, Status={Status}",
                    info.OutBillNo, record.Status);
                return true;
            }

            switch (info.State)
            {
                case "SUCCESS":
                    await HandleSuccessAsync(db, record, info, cancellationToken);
                    break;
                case "FAIL":
                case "CANCELLED":
                    await HandleFailureAsync(db, record, info, cancellationToken);
                    break;
                default:
                    logger.LogWarning(
                        "未知的转账状态：State={State}, WithdrawalNumber={WithdrawalNumber}",
                        info.State, info.OutBillNo);
                    return true;
            }

            logger.LogInformation(
                "微信提现完成回调处理成功：WithdrawalNumber={WithdrawalNumber}, State={State}",
                info.OutBillNo, info.State);
        }
        catch (Exception ex)
        {
            var workerName = WorkerName;
            var body = Encoding.UTF8.GetString(eventArgs.Body.Span);
            var message = $"处理微信提现完成回调异常，消息内容：{body}";
            await OnHandleFailAsync(workerName, message);
            LogErrorHandle(logger, ex, body);
        }
        return true;
    }

    /// <summary>
    /// 处理转账成功
    /// </summary>
    private static async Task HandleSuccessAsync(
        AppDbContext db,
        PCUserWithdrawalRecord record,
        WithdrawalTransferCompletedInfo info,
        CancellationToken cancellationToken)
    {
        record.Status = PCUserWithdrawalStatus.Success;
        record.ThirdPartyPlatformNumber = info.TransferBillNo;
        record.FinishTime = DateTimeOffset.Now;
        record.Note = $"微信转账成功，转账单号：{info.TransferBillNo}";

        // 创建提现完成变更记录
        var changeRecord = new PCUserWalletChangeRecord
        {
            Id = Guid.NewGuid(),
            UserId = record.UserId,
            Type = PCUserWalletValueType.WithdrawnAmount,
            Event = PCUserWalletValueEvent.Withdrawal,
            Direction = UserWalletPaymentDirection.Out,
            ChangeValue = 0, // 金额已在申请时扣减，此处仅记录完成
            ResultValue = record.Amount, // 记录已提现金额
            Reason = "微信提现转账成功",
            CreateTime = DateTimeOffset.Now,
            SourceId = info.TransferBillNo,
        };
        db.PCUserWalletChangeRecords.Add(changeRecord);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 处理转账失败/取消，退回金额到钱包
    /// </summary>
    private static async Task HandleFailureAsync(
        AppDbContext db,
        PCUserWithdrawalRecord record,
        WithdrawalTransferCompletedInfo info,
        CancellationToken cancellationToken)
    {
        // 获取钱包
        var wallet = await db.PCUserWallets
            .FirstOrDefaultAsync(w => w.Id == record.UserId, cancellationToken);

        if (wallet == null)
        {
            throw new InvalidOperationException($"钱包不存在，UserId: {record.UserId}");
        }

        // 退回金额
        var refundAmount = record.Amount;
        wallet.WithdrawableAmount += refundAmount;
        wallet.WithdrawnAmount -= refundAmount;
        wallet.UpdateTime = DateTimeOffset.Now;

        // 更新提现记录
        record.Status = PCUserWithdrawalStatus.Failed;
        record.FailureReason = info.FailReason ?? info.State;
        record.FinishTime = DateTimeOffset.Now;
        record.Note = $"微信转账失败（{info.State}），已退回金额：{refundAmount} 元";

        // 创建提现退回变更记录
        var refundChangeRecord = new PCUserWalletChangeRecord
        {
            Id = Guid.NewGuid(),
            UserId = record.UserId,
            Type = PCUserWalletValueType.WithdrawableAmount,
            Event = PCUserWalletValueEvent.WithdrawalRefund,
            Direction = UserWalletPaymentDirection.In,
            ChangeValue = refundAmount,
            ResultValue = wallet.WithdrawableAmount,
            Reason = $"微信提现失败退回（{info.State}）",
            CreateTime = DateTimeOffset.Now,
            SourceId = info.TransferBillNo,
        };
        db.PCUserWalletChangeRecords.Add(refundChangeRecord);

        await db.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "处理微信提现完成回调异常，消息内容：{body}")]
    private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? body);
}
