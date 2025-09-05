using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

[Table("MerchantDeductionAgreementConfigurations")]
public partial class MerchantDeductionAgreementConfiguration :
    Entity<Guid>,
    ICreationTime,
    IUpdateTime,
    INote,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 编号
    /// </summary>
    [Comment("编号")]
    public required string Code { get; set; }

    /// <summary>
    /// 配置名
    /// </summary>
    [Comment("配置名")]
    public required string Name { get; set; }

    /// <summary>
    /// 模板 Id，微信支付签约需要配置此字段
    /// </summary>
    [Comment("模板 Id")]
    public string? PlanId { get; set; }

    /// <summary>
    /// 周期数
    /// </summary>
    [Comment("周期数")]
    public int Period { get; set; }

    /// <summary>
    /// 周期类型
    /// </summary>
    [StringLength(50)]
    [Comment("周期类型")]
    public required string PeriodType { get; set; }

    /// <summary>
    /// 初次扣款金额
    /// </summary>
    [Precision(18, 2)]
    [Comment("初次扣款金额")]
    public decimal FirstAmount { get; set; }

    /// <summary>
    /// 单次扣款金额
    /// </summary>
    [Precision(18, 2)]
    [Comment("单次扣款金额")]
    public decimal SingleAmount { get; set; }

    /// <summary>
    /// 平台类型
    /// </summary>
    [Comment("平台类型")]
    public PaymentType Platform { get; set; }

    /// <summary>
    /// 签约场景码。支付宝签约需要配置此字段
    /// </summary>
    [StringLength(50)]
    [Comment("签约场景码")]
    public string? SignScene { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Comment("业务类型")]
    public int BusinessTypeId { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Comment("更新时间")]
    public DateTimeOffset UpdateTime { get; set; }

    public virtual List<MerchantDeductionAgreement> MerchantDeductionAgreements { get; set; } = null!;

    //public virtual List<XunYouGood> XunYouGoods { get; set; } = null!;

    //public virtual List<XunYouGoodMDACRelation> XunYouGoodMDACRelations { get; set; } = null!;
}