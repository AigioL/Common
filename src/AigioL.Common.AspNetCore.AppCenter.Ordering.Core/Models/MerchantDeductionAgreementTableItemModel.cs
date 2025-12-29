using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class MerchantDeductionAgreementTableItemModel : IReadOnlyId<Guid>
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// UserId
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 签约时间
    /// </summary>
    public DateTimeOffset? SigningTime { get; set; }

    /// <summary>
    /// 解约时间
    /// </summary>
    public DateTimeOffset? UnSigningTime { get; set; }

    /// <summary>
    /// 平台类型
    /// </summary>
    public PaymentType Platform { get; set; }

    /// <summary>
    /// 签约协议号
    /// </summary>
    public string AgreementNo { get; set; } = "";

    /// <summary>
    /// 用户 OpenId
    /// </summary>
    public string? UserOpenId { get; set; }

    /// <summary>
    /// 用户登录账号
    /// </summary>
    public string? UserLoginAccount { get; set; }

    /// <summary>
    /// 外部协议号。支付宝协议号或微信支付委托代扣协议 Id
    /// </summary>
    public string ExtAgreementNo { get; set; } = "";

    /// <summary>
    /// 生效时间
    /// </summary>
    public DateTimeOffset? ValidTime { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    public DateTimeOffset? InvalidTime { get; set; }

    /// <summary>
    /// 签约场景码
    /// </summary>
    public string SignScene { get; set; } = "";

    /// <summary>
    /// 周期数
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// 周期类型
    /// </summary>
    public string PeriodType { get; set; } = "";

    /// <summary>
    /// 扣款执行日期
    /// </summary>
    public DateTimeOffset ExecuteTime { get; set; }

    /// <summary>
    /// 下次扣款时间
    /// </summary>
    public DateTimeOffset? NextDeductionTime { get; set; }

    /// <summary>
    /// 初次扣款金额
    /// </summary>
    public decimal FirstAmount { get; set; }

    /// <summary>
    /// 单次扣款金额
    /// </summary>
    public decimal SingleAmount { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public AgreementStatus Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public int BusinessTypeId { get; set; }

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string? UserNickName { get; set; }

    /// <summary>
    /// 通知状态
    /// </summary>
    public NoticeStatus NoticeStatus { get; set; }

    /// <summary>
    /// 通知次数
    /// </summary>
    public int NoticeCount { get; set; }

    /// <summary>
    /// 通知完成时间
    /// </summary>
    public DateTimeOffset? NoticeFinishTime { get; set; }

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
}
