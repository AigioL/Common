using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;

/// <summary>
/// 管理后台实体接口（租户与软删除）
/// </summary>
public interface ITenantBaseEntity :
    IEntity,
    ISoftDeleted,
    ITenantId
{
}

/// <inheritdoc cref="IOperatorBaseEntity"/>
public interface ITenantBaseEntity<[DynamicallyAccessedMembers(DAMT)] TPrimaryKey> :
    ITenantBaseEntity,
    IEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
}