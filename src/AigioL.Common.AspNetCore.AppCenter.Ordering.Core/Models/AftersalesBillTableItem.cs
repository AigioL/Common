using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class AftersalesBillTableItem : IReadOnlyId<Guid>, ICreateTime
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 售后单号
    /// </summary>
    public string AftersalesNumber { get; set; } = string.Empty;

    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    #region 用户信息

    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string? UserNickName { get; set; } = string.Empty;

    /// <summary>
    /// 手机号码
    /// </summary>
    public string? UserPhoneNumber { get; set; }

    public string? UserPhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? UserEmail { get; set; }

    #endregion

    /// <summary>
    /// 退款金额
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 审核状态
    /// </summary>
    public AuditStatus AuditStatus { get; set; }

    /// <summary>
    /// 退款原因
    /// </summary>
    public string RefundReason { get; set; } = "";

    /// <summary>
    /// 卖家备注
    /// </summary>
    public string SellerNote { get; set; } = "";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 创建人 UserId（创建此条目的后台管理员）
    /// </summary>
    public Guid? CreateUserId { get; set; }

    /// <summary>
    /// 创建人（创建此条目的后台管理员）
    /// </summary>
    public string? CreateUser { get; set; }

    /// <summary>
    /// 最后一次操作的人 UserId（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    /// 最后一次操作的人（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public string? OperatorUser { get; set; }

    /// <summary>
    /// 订单业务类型
    /// </summary>
    public int BusinessTypeId { get; set; }
}
