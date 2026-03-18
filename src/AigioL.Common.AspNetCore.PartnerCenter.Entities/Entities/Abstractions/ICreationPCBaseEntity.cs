using AigioL.Common.AspNetCore.PartnerCenter.Columns;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities.Abstractions;

/// <summary>
/// 合作伙伴后台实体接口（创建时间与创建合作伙伴后台用户）
/// </summary>
public interface ICreationPCBaseEntity :
    IEntity,
    ICreateTime,
    ICreatePCUser,
    ICreatePCUserIdNullable
{
    static void Configure<[DynamicallyAccessedMembers(DAMT)] TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ICreateTime, ICreatePCUser, ICreatePCUserIdNullable
    {
        builder.HasOne(x => x.CreatePCUser)
            .WithMany()
            .HasForeignKey(p => p.CreatePCUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <inheritdoc cref="ICreationPCBaseEntity"/>
public interface ICreationPCBaseEntity<[DynamicallyAccessedMembers(DAMT)] TPrimaryKey> :
    ICreationPCBaseEntity,
    IEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
}
