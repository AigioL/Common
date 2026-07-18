using System.ComponentModel;

namespace AigioL.Common.AspNetCore.PartnerCenter.Models;

/// <summary>
/// PC 用户提现状态
/// </summary>
public enum PCUserWithdrawalStatus : byte
{
    /// <summary>
    /// 待处理
    /// </summary>
    [Description("待处理")]
    Pending = 1,

    /// <summary>
    /// 处理中
    /// </summary>
    [Description("处理中")]
    Processing,

    /// <summary>
    /// 提现成功
    /// </summary>
    [Description("提现成功")]
    Success,

    /// <summary>
    /// 提现失败
    /// </summary>
    [Description("提现失败")]
    Failed,
}
