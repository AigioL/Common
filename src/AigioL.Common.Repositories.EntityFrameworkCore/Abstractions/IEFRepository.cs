using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

/// <summary>
/// 由 EntityFrameworkCore 实现的仓储接口
/// </summary>
public interface IEFRepository
{
    /// <summary>
    /// 获取当前的 DbContext
    /// </summary>
    DbContext DbContext { get; }

    /// <summary>
    /// 获取表名
    /// </summary>
    string TableName { get; }

    /// <inheritdoc cref="DbContext.SaveChangesAsync(CancellationToken)"/>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
       => DbContext.SaveChangesAsync(cancellationToken);

    /// <inheritdoc cref="DbContext.SaveChangesAsync(bool, CancellationToken)"/>
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
       => DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
}