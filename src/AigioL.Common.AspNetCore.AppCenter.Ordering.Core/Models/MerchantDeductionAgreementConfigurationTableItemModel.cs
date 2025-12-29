using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class MerchantDeductionAgreementConfigurationTableItemModel : IReadOnlyId<Guid>, INote
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// 编号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 配置名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板 Id
    /// </summary>
    public string? PlanId { get; set; }

    /// <summary>
    /// 周期数
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// 周期类型
    /// </summary>
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
    public string? SignScene { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public int BusinessTypeId { get; set; }

    /// <inheritdoc/>
    public string? Note { get; set; }

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
