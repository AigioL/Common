using System.ComponentModel;

namespace AigioL.Common.AspNetCore.PartnerCenter.Models;

/// <summary>
/// PC 用户结算状态
/// </summary>
public enum PCUserSettlementStatus : byte
{
    /// <summary>
    /// 待结算
    /// </summary>
    [Description("待结算")]
    Pending = 1,

    /// <summary>
    /// 已结算
    /// </summary>
    [Description("已结算")]
    Completed,

    /// <summary>
    /// 结算失败
    /// </summary>
    [Description("结算失败")]
    Failed,
}
