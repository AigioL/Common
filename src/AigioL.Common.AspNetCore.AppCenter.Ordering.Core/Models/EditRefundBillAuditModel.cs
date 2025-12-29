namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class EditRefundBillAuditModel
{
    /// <summary>
    /// 审核状态
    /// </summary>
    public RefundStatus RefundStatus { get; set; }

    /// <summary>
    /// 强制退款（用于跳过一些验证，如：参与过会员抽奖）
    /// </summary>
    public bool? ForceRefund { get; set; }
}
