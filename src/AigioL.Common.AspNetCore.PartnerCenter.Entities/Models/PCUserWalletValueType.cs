using System.ComponentModel;

namespace AigioL.Common.AspNetCore.PartnerCenter.Models;

/// <summary>
/// PC 用户钱包值类型
/// </summary>
public enum PCUserWalletValueType : byte
{
    /// <summary>
    /// 可提现金额
    /// </summary>
    [Description("可提现金额")]
    WithdrawableAmount = 1,

    /// <summary>
    /// 已提现金额
    /// </summary>
    [Description("已提现金额")]
    WithdrawnAmount,
}
