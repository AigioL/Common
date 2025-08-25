using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;

/// <summary>
/// 管理后台实体基类（租户与软删除）
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public abstract class TenantBaseEntity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    OperatorBaseEntity<TPrimaryKey>,
    ITenantBaseEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }

    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }
}
