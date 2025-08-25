using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;

/// <summary>
/// 管理后台实体基类（修改时间与操作后台用户）
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public abstract class OperatorBaseEntity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    CreationBaseEntity<TPrimaryKey>,
    IOperatorBaseEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    /// <inheritdoc/>
    [Comment("更新时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <inheritdoc/>
    [Comment("操作人")]
    public Guid? OperatorUserId { get; set; }

    /// <inheritdoc/>
    public virtual BMUser? OperatorUser { get; set; }

    public new abstract class EntityTypeConfiguration<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity> : CreationBaseEntity<TPrimaryKey>.EntityTypeConfiguration<TEntity>
         where TEntity : OperatorBaseEntity<TPrimaryKey>
    {
        /// <inheritdoc/>
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);
            IOperatorBaseEntity.Configure(builder);
        }
    }
}
