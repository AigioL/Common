using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.Primitives.Entities.Abstractions;

/// <summary>
/// 实体模型基类（数据库表）
/// </summary>
public abstract class Entity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    IEntity<TPrimaryKey> where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    [global::System.ComponentModel.DataAnnotations.Key]
    [Comment("主键")]
    public virtual TPrimaryKey Id { get; set; } = default!;
}
