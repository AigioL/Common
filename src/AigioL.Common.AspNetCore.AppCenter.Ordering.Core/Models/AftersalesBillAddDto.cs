namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 创建售后单模型
/// </summary>
public sealed partial class AftersalesBillAddDto
{
    /// <summary>
    /// 订单 Id
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// 退款原因
    /// </summary>
    public required string RefundReason { get; set; }
}
