using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserWalletChangeRecordModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public UserWalletValueType Type { get; set; }

    public UserWalletValueEvent Event { get; set; }

    public UserWalletPaymentDirection Direction { get; set; }

    public decimal ChangeValue { get; set; }

    public decimal ResultValue { get; set; }

    public string? Reason { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreateTime { get; set; }

    public string? SourceId { get; set; }

    public bool NoticeStatus { get; set; }
}
