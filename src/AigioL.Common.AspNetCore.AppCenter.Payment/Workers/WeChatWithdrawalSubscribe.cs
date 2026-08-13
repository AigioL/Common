#if USE_PC_USER_WITHDRAWAL
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Workers;

/// <summary>
/// PC 用户微信提现请求订阅 Worker
/// <para>消费提现申请消息，调用微信商家转账接口，并将结果缓存到 Redis 供前端轮询</para>
/// </summary>
public partial class WeChatWithdrawalSubscribe : WorkerBackgroundService
{
    readonly IServiceProvider serviceProvider;

    /// <summary>
    /// PC 提现状态 Redis 键前缀
    /// </summary>
    const string PCWithdrawalRedisKeyPrefix = CacheKeys.PCWithdrawalRedisKeyPrefix;

    /// <summary>
    /// 提现状态 Redis 缓存过期时间（24 小时，与微信转账单据过期时间一致）
    /// </summary>
    static readonly TimeSpan PCWithdrawalRedisExpiry = TimeSpan.FromHours(24);

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
    protected override string RoutingKey => CacheKeys.PCUserWeChatWithdrawalRequest;

    /// <inheritdoc/>
    protected override string QueueName => $"{CacheKeys.OrderQueueName}.{RoutingKey}";

    /// <inheritdoc/>
    protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        string? withdrawalNumber = null;
        try
        {
            withdrawalNumber = Encoding.UTF8.GetString(eventArgs.Body.Span);
            logger.LogInformation("收到 PC 微信提现请求：WithdrawalNumber={WithdrawalNumber}", withdrawalNumber);

            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var weChatPayService = scope.ServiceProvider.GetRequiredService<IWeChatPayServices>();
            var redisConn = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();

            // 1. 查询提现记录
            var record = await db.PCUserWithdrawalRecords
                .FirstOrDefaultAsync(r => r.WithdrawalNumber == withdrawalNumber, cancellationToken);

            if (record == null)
            {
                logger.LogWarning("未找到提现记录：WithdrawalNumber={WithdrawalNumber}", withdrawalNumber);
                return true;
            }

            // 避免重复处理
            if (record.Status != PCUserWithdrawalStatus.Pending)
            {
                logger.LogInformation(
                    "提现记录非待处理状态，跳过：WithdrawalNumber={WithdrawalNumber}, Status={Status}",
                    withdrawalNumber, record.Status);
                return true;
            }

            // 2. 更新状态为处理中
            record.Status = PCUserWithdrawalStatus.Processing;
            await db.SaveChangesAsync(cancellationToken);

            // 3. 调用微信转账接口
            var transferRemark = "推广分成提现";
            var result = await weChatPayService.Transfer(
                record.WithdrawalNumber,
                record.Amount,
                transferRemark,
                record.UserOpenId!);

            // 4. 根据微信返回结果处理
            if (!result.IsSuccess)
            {
                // 5. 缓存转账状态到 Redis
                await CacheTransferStateAsync(redisConn, withdrawalNumber, result.WeChatState, null);
                // API 调用失败，退回金额
                await HandleTransferApiFailureAsync(db, record, result.Message ?? "微信转账接口调用失败", cancellationToken);
                return true;
            }

            // 5. 缓存转账状态到 Redis
            await CacheTransferStateAsync(redisConn, withdrawalNumber, result.WeChatState, result.PackageInfo);

            // 6. 根据状态更新提现记录
            switch (result.WeChatState)
            {
                case "SUCCESS":
                    record.Status = PCUserWithdrawalStatus.Success;
                    record.ThirdPartyPlatformNumber = result.ThirdPartyPlatformNumber;
                    record.FinishTime = DateTimeOffset.Now;
                    break;

                case "FAIL":
                case "CANCELLED":
                    await HandleTransferFailureAsync(db, record, result.Message ?? result.WeChatState, cancellationToken);
                    return true;

                case "WAIT_USER_CONFIRM":
                case "ACCEPTED":
                case "PROCESSING":
                case "TRANSFERING":
                case "CANCELING":
                    // 非终态，保持 Processing 状态，等待回调或用户确认
                    record.ThirdPartyPlatformNumber = result.ThirdPartyPlatformNumber;
                    record.Note = $"转账处理中（{result.WeChatState}），等待用户确认或回调";
                    break;

                default:
                    logger.LogWarning(
                        "未知的微信转账状态：State={State}, WithdrawalNumber={WithdrawalNumber}",
                        result.WeChatState, withdrawalNumber);
                    break;
            }

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "PC 微信提现处理完成：WithdrawalNumber={WithdrawalNumber}, State={WeChatState}, Status={Status}",
                withdrawalNumber, result.WeChatState, record.Status);
        }
        catch (Exception ex)
        {
            var workerName = WorkerName;
            var body = Encoding.UTF8.GetString(eventArgs.Body.Span);
            var message = $"处理微信提现请求异常，消息内容：{body}";
            await OnHandleFailAsync(workerName, message);
            LogErrorHandle(logger, ex, body);
        }
        return true;
    }

    /// <summary>
    /// 缓存转账状态到 Redis
    /// </summary>
    private static async Task CacheTransferStateAsync(
        IConnectionMultiplexer redisConn,
        string withdrawalNumber,
        string? state,
        string? packageInfo)
    {
        var db = redisConn.GetDatabase();
        var key = $"{PCWithdrawalRedisKeyPrefix}{withdrawalNumber}";

        var cacheData = new PCWithdrawalTransferState
        {
            State = state ?? string.Empty,
            PackageInfo = packageInfo,
        };

        var serialized = MemoryPack.MemoryPackSerializer.Serialize(cacheData);
        await db.StringSetAsync(key, serialized, PCWithdrawalRedisExpiry);
    }

    /// <summary>
    /// 处理转账失败，退回金额到钱包
    /// </summary>
    private static async Task HandleTransferFailureAsync(
        AppDbContext db,
        PCUserWithdrawalRecord record,
        string failureReason,
        CancellationToken cancellationToken)
    {
        var wallet = await db.PCUserWallets
            .FirstOrDefaultAsync(w => w.Id == record.UserId, cancellationToken);

        if (wallet == null)
        {
            throw new InvalidOperationException($"钱包不存在，UserId: {record.UserId}");
        }

        var refundAmount = record.Amount;
        wallet.WithdrawableAmount += refundAmount;
        wallet.WithdrawnAmount -= refundAmount;
        wallet.UpdateTime = DateTimeOffset.Now;

        record.Status = PCUserWithdrawalStatus.Failed;
        record.FailureReason = failureReason;
        record.FinishTime = DateTimeOffset.Now;
        record.Note = $"微信转账失败：{failureReason}，已退回 {refundAmount} 元";

        var refundChangeRecord = new PCUserWalletChangeRecord
        {
            Id = Guid.NewGuid(),
            UserId = record.UserId,
            Type = PCUserWalletValueType.WithdrawableAmount,
            Event = PCUserWalletValueEvent.WithdrawalRefund,
            Direction = UserWalletPaymentDirection.In,
            ChangeValue = refundAmount,
            ResultValue = wallet.WithdrawableAmount,
            Reason = $"微信提现失败退回（{failureReason}）",
            CreateTime = DateTimeOffset.Now,
            SourceId = $"{record.WithdrawalNumber}_REFUND",
        };
        db.PCUserWalletChangeRecords.Add(refundChangeRecord);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 处理微信 API 调用失败，退回金额到钱包
    /// </summary>
    private static async Task HandleTransferApiFailureAsync(
        AppDbContext db,
        PCUserWithdrawalRecord record,
        string failureReason,
        CancellationToken cancellationToken)
    {
        var wallet = await db.PCUserWallets
            .FirstOrDefaultAsync(w => w.Id == record.UserId, cancellationToken);

        if (wallet == null)
        {
            throw new InvalidOperationException($"钱包不存在，UserId: {record.UserId}");
        }

        var refundAmount = record.Amount;
        wallet.WithdrawableAmount += refundAmount;
        wallet.WithdrawnAmount -= refundAmount;
        wallet.UpdateTime = DateTimeOffset.Now;

        record.Status = PCUserWithdrawalStatus.Failed;
        record.FailureReason = failureReason;
        record.FinishTime = DateTimeOffset.Now;
        record.Note = $"微信转账接口调用失败：{failureReason}，已退回 {refundAmount} 元";

        var refundChangeRecord = new PCUserWalletChangeRecord
        {
            Id = Guid.NewGuid(),
            UserId = record.UserId,
            Type = PCUserWalletValueType.WithdrawableAmount,
            Event = PCUserWalletValueEvent.WithdrawalRefund,
            Direction = UserWalletPaymentDirection.In,
            ChangeValue = refundAmount,
            ResultValue = wallet.WithdrawableAmount,
            Reason = $"微信提现API失败退回（{failureReason}）",
            CreateTime = DateTimeOffset.Now,
            SourceId = $"{record.WithdrawalNumber}_API_FAIL",
        };
        db.PCUserWalletChangeRecords.Add(refundChangeRecord);

        await db.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "处理微信提现请求异常，消息内容：{body}")]
    private static partial void LogErrorHandle(ILogger logger, Exception? ex, string? body);
}
#endif