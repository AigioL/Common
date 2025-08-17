using AigioL.Common.AspNetCore.AdminCenter.Columns;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;

/// <summary>
/// 管理后台实体接口（修改时间与操作后台用户）
/// </summary>
public interface IOperatorBaseEntity :
    IEntity,
    IUpdateTime,
    IOperatorBMUser,
    IOperatorUserId
{
    static void Configure<[DynamicallyAccessedMembers(DAMT)] TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IOperatorBaseEntity
    {
        builder.HasOne(x => x.OperatorUser)
            .WithMany()
            .HasForeignKey(p => p.OperatorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <inheritdoc cref="IOperatorBaseEntity"/>
public interface IOperatorBaseEntity<[DynamicallyAccessedMembers(DAMT)] TPrimaryKey> :
    IOperatorBaseEntity,
    IEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
}