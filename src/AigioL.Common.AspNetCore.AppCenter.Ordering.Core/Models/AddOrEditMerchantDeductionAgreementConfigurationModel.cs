using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class AddOrEditMerchantDeductionAgreementConfigurationModel : IReadOnlyId<Guid>, INote
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// 编号
    /// </summary>
    [StringLength(255)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 配置名
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板 Id
    /// </summary>
    [StringLength(255)]
    public string? PlanId { get; set; }

    /// <summary>
    /// 周期数
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// 周期类型
    /// </summary>
    [StringLength(50)]
    public string PeriodType { get; set; } = string.Empty;

    /// <summary>
    /// 初次扣款金额
    /// </summary>
    public decimal FirstAmount { get; set; }

    /// <summary>
    /// 单次扣款金额
    /// </summary>
    public decimal SingleAmount { get; set; }

    /// <summary>
    /// 平台类型
    /// </summary>
    public PaymentType Platform { get; set; }

    /// <summary>
    /// 签约场景码
    /// </summary>
    [StringLength(50)]
    public string? SignScene { get; set; }

    /// <summary>
    /// 业务类型 Id
    /// </summary>
    public int BusinessTypeId { get; set; }

    public string? Note { get; set; }
}
