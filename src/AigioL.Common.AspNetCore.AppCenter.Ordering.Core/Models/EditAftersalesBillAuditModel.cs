using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 修改售后单审核状态模型
/// </summary>
public sealed partial class EditAftersalesBillAuditModel
{
    /// <summary>
    /// 审核状态
    /// </summary>
    public AuditStatus AuditStatus { get; set; }

    /// <summary>
    /// 卖家备注
    /// </summary>
    [StringLength(2000)]
    public string SellerNote { get; set; } = "";
}

#if DEBUG
[Obsolete("use EditAftersalesBillAuditModel", true)]
public partial class AftersalesAuditDTO;
#endif