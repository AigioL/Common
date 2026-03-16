using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class OrderTableItem : IReadOnlyId<string>, INote
{
    /// <inheritdoc/>
    public required string Id { get; set; }

    public string? OpenId { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNumber => Id;

    /// <summary>
    /// 订单类型
    /// </summary>
    public OrderType Type { get; set; }

    /// <summary>
    /// 订单来源终端
    /// </summary>
    public DevicePlatform2 Source { get; set; }

    /// <summary>
    /// 订单超时时间
    /// </summary>
    public DateTimeOffset Timeout { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid UserId { get; set; }

    #region 用户信息

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
    /// 应收金额
    /// </summary>
    public decimal AmountReceivable { get; set; }

    /// <summary>
    /// 实收金额
    /// </summary>
    public decimal AmountReceived { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    public DateTimeOffset? PaymentTime { get; set; }

    /// <summary>
    /// 支付类型
    /// </summary>
    public PaymentType? PaymentType { get; set; }

    /// <summary>
    /// 业务类型，关联的支付业务类型枚举
    /// </summary>
    public int BusinessTypeId { get; set; }

    /// <summary>
    /// 业务 Id，业务类型订单的 Id
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <inheritdoc/>
    public string? Note { get; set; }

    /// <summary>
    /// 商家扣款协议 Id
    /// </summary>
    public Guid? MerchantDeductionAgreementId { get; set; }

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
    /// 渠道包 Id
    /// </summary>
    public Guid? ChannelPackageId { get; set; }
    /// <summary>
    /// 绑定的合作伙伴用户 Id
    /// </summary>
    public Guid? BindPCUserId { get; set; }
}
