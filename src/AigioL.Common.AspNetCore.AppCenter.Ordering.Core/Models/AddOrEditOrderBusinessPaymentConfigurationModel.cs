using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class AddOrEditOrderBusinessPaymentConfigurationModel : IReadOnlyId<Guid>, ISort, IDisable
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public int BusinessTypeId { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 支付类型
    /// </summary>
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disable { get; set; }

    /// <inheritdoc/>
    public long Sort { get; set; }
}
