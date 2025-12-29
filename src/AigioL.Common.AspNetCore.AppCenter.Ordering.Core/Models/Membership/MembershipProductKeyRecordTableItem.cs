using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

public sealed partial class MembershipProductKeyRecordTableItem : IReadOnlyId<Guid>, ICreateTime
{
    public Guid Id { get; set; }

    public string Key { get; set; } = default!;

    public string ProductName { get; set; } = string.Empty;

    public DateTimeOffset? UsageTime { get; set; }

    public int RechargeDays { get; set; }

    public bool IsUsed { get; set; }

    public bool Disable { get; set; }

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
