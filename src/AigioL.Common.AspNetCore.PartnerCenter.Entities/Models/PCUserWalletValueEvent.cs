using System.ComponentModel;

namespace AigioL.Common.AspNetCore.PartnerCenter.Models;

/// <summary>
/// PC 用户钱包值事件
/// </summary>
public enum PCUserWalletValueEvent : byte
{
    /// <summary>
    /// 月度结算
    /// </summary>
    [Description("月度结算")]
    MonthlySettlement = 1,

    /// <summary>
    /// 提现
    /// </summary>
    [Description("提现")]
    Withdrawal,

    /// <summary>
    /// 提现退回
    /// </summary>
    [Description("提现退回")]
    WithdrawalRefund,
}
