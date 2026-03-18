using AigioL.Common.AspNetCore.PartnerCenter.Columns;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities.Abstractions;

/// <summary>
/// 合作伙伴后台实体接口（修改时间与操作合作伙伴后台用户）
/// </summary>
public interface IOperatorPCBaseEntity :
    IEntity,
    IUpdateTime,
    IOperatorPCUser,
    IOperatorPCUserId
{
    static void Configure<[DynamicallyAccessedMembers(DAMT)] TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IOperatorPCBaseEntity
    {
        builder.HasOne(x => x.OperatorPCUser)
            .WithMany()
            .HasForeignKey(p => p.OperatorPCUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <inheritdoc cref="IOperatorPCBaseEntity"/>
public interface IOperatorPCBaseEntity<[DynamicallyAccessedMembers(DAMT)] TPrimaryKey> :
    IOperatorPCBaseEntity,
    IEntity<TPrimaryKey>
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
}