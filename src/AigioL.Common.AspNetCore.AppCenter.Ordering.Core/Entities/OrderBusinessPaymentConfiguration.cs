using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 业务类型支付配置表实体类
/// </summary>
[Table("OrderBusinessPaymentConfigurations")]
public partial class OrderBusinessPaymentConfiguration :
    OperatorBaseEntity<Guid>,
    ITenantId,
    IDisable,
    INEWSEQUENTIALID
{
    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Comment("业务类型")]
    public int BusinessTypeId { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    [Comment("支付方式")]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 支付类型
    /// </summary>
    [Comment("支付类型")]
    public PaymentType PaymentType { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }
}