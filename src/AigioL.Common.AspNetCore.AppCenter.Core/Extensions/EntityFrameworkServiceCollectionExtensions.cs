using AigioL.Common.EntityFrameworkCore.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class EntityFrameworkServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddDbContext2<[
        DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(
        this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped,
        string? databaseProvider = SqlStringHelper.PostgreSQL,
        bool? postgreSQL18Plus = true)
        where TContext : DbContext
    {
        switch (databaseProvider)
        {
            case SqlStringHelper.PostgreSQL:
                {
                    SqlStringHelper.ConfigPostgreSQL(postgreSQL18Plus);
                }
                break;
        }
        return serviceCollection.AddDbContext<TContext>(optionsAction, contextLifetime, optionsLifetime);
    }
}
