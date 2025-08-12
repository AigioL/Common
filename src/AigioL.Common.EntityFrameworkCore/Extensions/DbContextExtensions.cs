using AigioL.Common.EntityFrameworkCore.Helpers;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.EntityFrameworkCore.Extensions;

public static partial class DbContextExtensions
{
    /// <summary>
    /// 根据类型获取表名称
    /// </summary>
    /// <param name="database"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public static string GetTableNameByClrType(this DatabaseFacade database, IEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        ArgumentNullException.ThrowIfNull(tableName);
        var schema = entityType.GetSchema();
        var databaseProviderName = database.ProviderName;
        return databaseProviderName switch
        {
            SqlStringHelper.SqlServer => $"[{(string.IsNullOrEmpty(schema) ? "dbo" : schema)}].[{tableName}]",
            SqlStringHelper.PostgreSQL => string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"",
            _ => throw ThrowHelper.GetArgumentOutOfRangeException(databaseProviderName),
        };
    }

    /// <summary>
    /// 根据类型获取表名称
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityClrType"></param>
    /// <returns></returns>
    public static string GetTableNameByClrType(this DbContext context, [DynamicallyAccessedMembers(IEntity.DAMT)] Type entityClrType)
    {
        var entityType = context.Model.FindEntityType(entityClrType);
        ArgumentNullException.ThrowIfNull(entityType);
        return context.Database.GetTableNameByClrType(entityType);
    }
}
