using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;

/// <summary>
/// 管理后台实体基类（创建时间与创建后台用户）
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public abstract class CreationBaseEntity<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> :
    Entity<TPrimaryKey>,
    ICreationBaseEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建人")]
    public Guid? CreateUserId { get; set; }

    /// <inheritdoc/>
    public virtual BMUser? CreateUser { get; set; }

    public abstract class EntityTypeConfiguration<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity> :
        IEntityTypeConfiguration<TEntity>
        where TEntity : CreationBaseEntity<TPrimaryKey>
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            ICreationBaseEntity.Configure(builder);
        }
    }
}
