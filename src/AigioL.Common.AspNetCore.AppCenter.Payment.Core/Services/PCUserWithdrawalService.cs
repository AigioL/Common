using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

/// <summary>
/// PC 用户提现服务实现
/// </summary>
public sealed partial class PCUserWithdrawalService : IPCUserWithdrawalService
{
    readonly AppDbContext db;
    readonly IKeyValuePairRepository kvRepo;
    readonly ILogger<PCUserWithdrawalService> logger;

    public PCUserWithdrawalService(
        AppDbContext db,
        IKeyValuePairRepository kvRepo,
        ILogger<PCUserWithdrawalService> logger)
    {
        this.db = db;
        this.kvRepo = kvRepo;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<PCUserWithdrawalResponseModel>> ApplyWithdrawalAsync(
        PCUserWithdrawalRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // 1. 获取提现配置
        var maxPerWithdrawalStr = await kvRepo.QueryValueAsync(CacheKeys.PCUser单次提现最大金额, cancellationToken);
        var maxPerWithdrawal = decimal.TryParse(maxPerWithdrawalStr, out var mpw) ? mpw : CacheKeys.PCUser单次提现最大金额默认值;

        var maxDailyPerAccountStr = await kvRepo.QueryValueAsync(CacheKeys.PCUser单账号每日提现上限, cancellationToken);
        var maxDailyPerAccount = decimal.TryParse(maxDailyPerAccountStr, out var mdpa) ? mdpa : CacheKeys.PCUser单账号每日提现上限默认值;

        var maxDailyTotalStr = await kvRepo.QueryValueAsync(CacheKeys.PCUser单日总金额提现上限, cancellationToken);
        var maxDailyTotal = decimal.TryParse(maxDailyTotalStr, out var mdt) ? mdt : CacheKeys.PCUser单日总金额提现上限默认值;

        // 2. 校验单次提现上限
        if (request.Amount > maxPerWithdrawal)
        {
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>($"单次提现金额不能超过 {maxPerWithdrawal} 元");
        }

        if (request.Amount <= 0)
        {
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>("提现金额必须大于 0");
        }

        // 3. 获取钱包（带行版本乐观锁检查）
        var wallet = await db.PCUserWallets
            .FirstOrDefaultAsync(w => w.Id == request.UserId, cancellationToken);

        if (wallet == null)
        {
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>("钱包不存在");
        }

        // 4. 检查可提现余额
        if (wallet.WithdrawableAmount < request.Amount)
        {
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>("可提现余额不足");
        }

        // 5. 检查当日该账号已提现金额
        var today = DateTimeOffset.Now.Date;
        var todayEnd = today.AddDays(1);

        var todayAccountWithdrawn = await db.PCUserWithdrawalRecords
            .Where(r => r.UserId == request.UserId
                && r.CreateTime >= today
                && r.CreateTime < todayEnd
                && r.Status != PCUserWithdrawalStatus.Failed)
            .SumAsync(r => r.Amount, cancellationToken);

        if (todayAccountWithdrawn + request.Amount > maxDailyPerAccount)
        {
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>(
                $"单账号每日提现上限为 {maxDailyPerAccount} 元，今日已提现 {todayAccountWithdrawn} 元");
        }

        // 6. 检查当日平台总提现金额
        var todayTotalWithdrawn = await db.PCUserWithdrawalRecords
            .Where(r => r.CreateTime >= today
                && r.CreateTime < todayEnd
                && r.Status != PCUserWithdrawalStatus.Failed)
            .SumAsync(r => r.Amount, cancellationToken);

        if (todayTotalWithdrawn + request.Amount > maxDailyTotal)
        {
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>(
                $"平台单日总提现上限为 {maxDailyTotal} 元，今日已提现 {todayTotalWithdrawn} 元");
        }

        // 7. 生成提现单号
        var withdrawalNumber = GenerateWithdrawalNumber();

        // 8. 使用事务：扣减钱包 + 创建提现记录 + 创建变更记录
        using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // 扣减可提现金额，增加已提现金额
            wallet.WithdrawableAmount -= request.Amount;
            wallet.WithdrawnAmount += request.Amount;
            wallet.UpdateTime = DateTimeOffset.Now;

            // 创建提现记录
            var withdrawalRecord = new PCUserWithdrawalRecord
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                WithdrawalNumber = withdrawalNumber,
                Amount = request.Amount,
                Status = PCUserWithdrawalStatus.Pending,
                PaymentPlatform = PaymentType.WeChatPay,
                UserOpenId = request.WeChatOpenId,
                CreateTime = DateTimeOffset.Now,
                Note = request.Note,
            };
            db.PCUserWithdrawalRecords.Add(withdrawalRecord);

            // 创建钱包变更记录
            var changeRecord = new PCUserWalletChangeRecord
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Type = PCUserWalletValueType.WithdrawableAmount,
                Event = PCUserWalletValueEvent.Withdrawal,
                Direction = AigioL.Common.AspNetCore.AppCenter.Models.UserWalletPaymentDirection.Out,
                ChangeValue = -request.Amount,
                ResultValue = wallet.WithdrawableAmount,
                Reason = "提现申请",
                CreateTime = DateTimeOffset.Now,
                SourceId = withdrawalNumber,
            };
            db.PCUserWalletChangeRecords.Add(changeRecord);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // TODO: 发送微信提现消息到 RabbitMQ

            return ApiRsp.Ok(new PCUserWithdrawalResponseModel
            {
                WithdrawalNumber = withdrawalNumber,
                Amount = request.Amount,
                Status = PCUserWithdrawalStatus.Pending,
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>("钱包数据并发冲突，请稍后重试");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "提现申请失败，UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>($"提现申请失败: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<PCUserWalletInfoModel>> GetWalletInfoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var wallet = await db.PCUserWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == userId, cancellationToken);

        if (wallet == null)
        {
            return ApiRsp.Fail<PCUserWalletInfoModel>("钱包不存在");
        }

        return ApiRsp.Ok(new PCUserWalletInfoModel
        {
            UserId = wallet.Id,
            WithdrawableAmount = wallet.WithdrawableAmount,
            WithdrawnAmount = wallet.WithdrawnAmount,
            CumulativeSettlementAmount = wallet.CumulativeSettlementAmount,
        });
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<PCUserWithdrawalResponseModel>> GetWithdrawalAsync(
        string withdrawalNumber,
        CancellationToken cancellationToken = default)
    {
        var record = await db.PCUserWithdrawalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.WithdrawalNumber == withdrawalNumber, cancellationToken);

        if (record == null)
        {
            return ApiRsp.Fail<PCUserWithdrawalResponseModel>("提现记录不存在");
        }

        return ApiRsp.Ok(new PCUserWithdrawalResponseModel
        {
            WithdrawalNumber = record.WithdrawalNumber,
            Amount = record.Amount,
            Status = record.Status,
        });
    }

    /// <summary>
    /// 生成提现单号
    /// </summary>
    private static string GenerateWithdrawalNumber()
    {
        return $"PCW{DateTimeOffset.Now:yyyyMMddHHmmss}{Random.Shared.Next(10000, 99999)}";
    }
}
