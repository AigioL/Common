using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserWalletModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 余额
    /// </summary>
    public decimal AccountBalance { get; set; }

    /// <summary>
    /// 可提现金额
    /// </summary>
    public decimal WithdrawableAmount { get; set; }

    /// <summary>
    /// 可用金额
    /// </summary>
    public decimal AvailableAmount { get; set; }

    /// <summary>
    /// 累计充值金额
    /// </summary>
    public decimal CumulativeRechargeAmount { get; set; }

    /// <summary>
    /// 累计消费金额
    /// </summary>
    public decimal CumulativeConsumptionAmount { get; set; }

    /// <summary>
    /// 累计奖励金额
    /// </summary>
    public decimal CumulativeRewardAmount { get; set; }

    /// <summary>
    /// 累计收益金额
    /// </summary>
    public decimal CumulativeIncomeAmount { get; set; }

    /// <summary>
    /// 付费积分
    /// </summary>
    public long ProPoints { get; set; }

    /// <summary>
    /// 可用付费积分
    /// </summary>
    public long AvailableProPoints { get; set; }

    /// <summary>
    /// 免费积分
    /// <para>历史用户信息表中的 EngineOil 字段（机油、体力、疲劳值、积分1）</para>
    /// </summary>
    public long FreePoints { get; set; }
}
