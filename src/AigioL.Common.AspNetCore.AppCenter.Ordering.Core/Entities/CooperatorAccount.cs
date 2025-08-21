using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

public partial class CooperatorAccount :
    OperatorBaseEntity<Guid>,
    INote,
    IDisable,
    ISoftDeleted,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 名称
    /// </summary>
    [StringLength(20)]
    [Comment("名称")]
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// 应用 Id
    /// </summary>
    [StringLength(50)]
    [Comment("应用 Id")]
    [Required]
    public required string AppId { get; set; }

    /// <summary>
    /// 我方私钥
    /// </summary>
    [Comment("我方私钥")]
    [Required]
    public required string OurPrivateKey { get; set; }

    /// <summary>
    /// 合作者公钥
    /// </summary>
    [Comment("合作者公钥")]
    [Required]
    public required string CooperatorPublicKey { get; set; }

    /// <summary>
    /// 支付通知地址
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("支付通知地址")]
    public string? PaymentNoticeUrl { get; set; }

    /// <summary>
    /// 退款通知地址
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("退款通知地址")]
    public string? RefundNoticeUrl { get; set; }

    /// <summary>
    /// 商家扣款协议通知地址
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("商家扣款协议通知地址")]
    public string? AgreementNoticeUrl { get; set; }

    /// <summary>
    /// 白名单
    /// </summary>
    [Comment("白名单")]
    public string? IPWhitelist { get; set; }

    /// <summary>
    /// 退款是否需要审批
    /// </summary>
    [Comment("退款是否需要审批")]
    public bool RefundNeedAudit { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <summary>
    /// 是否软删除
    /// </summary>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }
}