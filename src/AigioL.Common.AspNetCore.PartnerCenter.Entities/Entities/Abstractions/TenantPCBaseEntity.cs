using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities.Abstractions;

/// <summary>
/// 合作伙伴后台实体基类（租户与软删除）
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public abstract class TenantPCBaseEntity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    OperatorPCBaseEntity<TPrimaryKey>,
    ITenantBaseEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    /// <inheritdoc/>
    [Comment("是否软删除")]
    public DateTimeOffset? DeleteTime { get; set; }

    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }
}
