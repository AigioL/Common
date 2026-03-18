using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities.Abstractions;

/// <summary>
/// 合作伙伴后台实体基类（创建时间与创建合作伙伴后台用户）
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public abstract class CreationPCBaseEntity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    Entity<TPrimaryKey>,
    ICreationPCBaseEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreateTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建人")]
    public Guid? CreatePCUserId { get; set; }

    /// <inheritdoc/>
    public virtual PCUser? CreatePCUser { get; set; }

    public abstract class EntityTypeConfiguration<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity> :
        IEntityTypeConfiguration<TEntity>
        where TEntity : CreationPCBaseEntity<TPrimaryKey>
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            ICreationPCBaseEntity.Configure(builder);
        }
    }
}
