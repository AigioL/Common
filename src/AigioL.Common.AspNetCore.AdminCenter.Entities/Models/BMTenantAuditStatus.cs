namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 租户审核状态
/// </summary>
public enum BMTenantAuditStatus : byte
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 待审核
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 审核中
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// 审核不通过
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// 审核通过
    /// </summary>
    Approved = byte.MaxValue,
}
