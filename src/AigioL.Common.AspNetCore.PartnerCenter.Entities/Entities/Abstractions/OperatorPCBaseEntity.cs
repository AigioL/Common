using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities.Abstractions;

/// <summary>
/// 合作伙伴后台实体基类（修改时间与操作合作伙伴后台用户）
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public abstract class OperatorPCBaseEntity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    CreationPCBaseEntity<TPrimaryKey>,
    IOperatorPCBaseEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    /// <inheritdoc/>
    [Comment("更新时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <inheritdoc/>
    [Comment("操作人")]
    public Guid? OperatorPCUserId { get; set; }

    /// <inheritdoc/>
    public virtual PCUser? OperatorPCUser { get; set; }

    public new abstract class EntityTypeConfiguration<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity> : CreationPCBaseEntity<TPrimaryKey>.EntityTypeConfiguration<TEntity>
         where TEntity : OperatorPCBaseEntity<TPrimaryKey>
    {
        /// <inheritdoc/>
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);
            IOperatorPCBaseEntity.Configure(builder);
        }
    }
}
