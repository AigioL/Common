using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.Primitives.Entities.Abstractions;

/// <summary>
/// 实体模型基类（数据库表）
/// </summary>
public abstract partial class Entity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    IEntity<TPrimaryKey> where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    [global::System.ComponentModel.DataAnnotations.Key]
    public virtual partial TPrimaryKey Id { get => field!; set; }
}

partial class Entity<TPrimaryKey>
{
    [Comment("主键")]
    public virtual partial TPrimaryKey Id { get; set; }
}