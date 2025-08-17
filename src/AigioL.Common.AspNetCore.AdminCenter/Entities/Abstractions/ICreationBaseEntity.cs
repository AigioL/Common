using AigioL.Common.AspNetCore.AdminCenter.Columns;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;

/// <summary>
/// 管理后台实体接口（创建时间与创建后台用户）
/// </summary>
public interface ICreationBaseEntity :
    IEntity,
    ICreationTime,
    ICreateBMUser,
    ICreateUserIdNullable
{
    static void Configure<[DynamicallyAccessedMembers(DAMT)] TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ICreationBaseEntity
    {
        builder.HasOne(x => x.CreateUser)
            .WithMany()
            .HasForeignKey(p => p.CreateUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <inheritdoc cref="ICreationBaseEntity"/>
public interface ICreationBaseEntity<[DynamicallyAccessedMembers(DAMT)] TPrimaryKey> :
    ICreationBaseEntity,
    IEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
}
