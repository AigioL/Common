using AigioL.Common.EntityFrameworkCore.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AigioL.Common.AspNetCore.AppCenter;

partial class MSMinimalApis
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
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                    if (postgreSQL18Plus.HasValue)
                    {
                        SqlStringHelper.PostgreSQL18Plus = postgreSQL18Plus.Value;
                    }
                }
                break;
        }
        return serviceCollection.AddDbContext<TContext>(optionsAction, contextLifetime, optionsLifetime);
    }
}
