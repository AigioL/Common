using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Jobs;

/// <summary>
/// PC 用户月度结算任务
/// 每月 10 号执行，将上月收益结算到 PC 用户钱包可提现金额
/// </summary>
public partial class MonthlySettlementJob(
    ILogger<MonthlySettlementJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) :
    JobService<AppDbContext, MonthlySettlementJob>(logger, dbContext, feishuApiClient)
{
    /// <inheritdoc/>
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        // 计算上个月的年月
        var lastMonth = DateTimeOffset.Now.AddMonths(-1);
        var settlementYearMonth = lastMonth.ToString("yyyy-MM");

        logger.LogInformation("开始执行 PC 用户月度结算，结算年月：{SettlementYearMonth}", settlementYearMonth);

        // 检查是否已结算过
        var alreadySettled = await dbContext.PCUserMonthlySettlements
            .AsNoTracking()
            .AnyAsync(s => s.SettlementYearMonth == settlementYearMonth
                && s.Status == PCUserSettlementStatus.Completed, cancellationToken);

        if (alreadySettled)
        {
            logger.LogInformation("结算年月 {SettlementYearMonth} 已结算过，跳过", settlementYearMonth);
            return true;
        }

        // TODO: 实现具体的结算逻辑
        // 1. 从 MembershipBusinessOrder / PromoCode 等数据源统计每个 PCUser 的上月收益
        // 2. 创建 PCUserMonthlySettlement 记录
        // 3. 更新 PCUserWallet.WithdrawableAmount 和 CumulativeSettlementAmount
        // 4. 创建 PCUserWalletChangeRecord 记录

        // 当前为占位实现，仅记录日志
        var settlementCount = 0;
        try
        {
            // 示例：获取所有有钱包的 PCUser
            var wallets = await dbContext.PCUserWallets
                .Where(w => true) // TODO: 替换为实际结算条件
                .Take(0) // 占位：先不处理任何数据
                .ToListAsync(cancellationToken);

            foreach (var wallet in wallets)
            {
                // TODO: 计算每个用户的结算金额
                // var settlementAmount = await CalculateSettlementAmountAsync(wallet.Id, lastMonth, cancellationToken);
                //
                // if (settlementAmount > 0)
                // {
                //     var settlement = new PCUserMonthlySettlement
                //     {
                //         Id = SequentialGuidGenerator.Create(),
                //         UserId = wallet.Id,
                //         SettlementYearMonth = settlementYearMonth,
                //         SettlementAmount = settlementAmount,
                //         Status = PCUserSettlementStatus.Completed,
                //         SettlementTime = DateTimeOffset.Now,
                //         CreateTime = DateTimeOffset.Now,
                //     };
                //     dbContext.PCUserMonthlySettlements.Add(settlement);
                //
                //     wallet.WithdrawableAmount += settlementAmount;
                //     wallet.CumulativeSettlementAmount += settlementAmount;
                //
                //     var changeRecord = new PCUserWalletChangeRecord
                //     {
                //         Id = SequentialGuidGenerator.Create(),
                //         UserId = wallet.Id,
                //         Type = PCUserWalletValueType.WithdrawableAmount,
                //         Event = PCUserWalletValueEvent.MonthlySettlement,
                //         Direction = AigioL.Common.AspNetCore.AppCenter.Models.UserWalletPaymentDirection.In,
                //         ChangeValue = settlementAmount,
                //         ResultValue = wallet.WithdrawableAmount,
                //         Reason = $"月度结算 {settlementYearMonth}",
                //         CreateTime = DateTimeOffset.Now,
                //         SourceId = settlement.Id.ToString(),
                //     };
                //     dbContext.PCUserWalletChangeRecords.Add(changeRecord);
                //
                //     settlementCount++;
                // }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("PC 用户月度结算完成，结算年月：{SettlementYearMonth}，结算人数：{Count}",
                settlementYearMonth, settlementCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PC 用户月度结算失败，结算年月：{SettlementYearMonth}", settlementYearMonth);
            throw;
        }

        return true;
    }
}
