using AigioL.Common.EntityFrameworkCore.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Runtime.CompilerServices;

namespace AigioL.Common.EntityFrameworkCore.Extensions;

public static partial class EntityPropertyBuilderExtensions
{
    /// <inheritdoc cref="NpgsqlPropertyBuilderExtensions.UseHiLo"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyBuilder<TProperty> UseHiLo2<TProperty>(this PropertyBuilder<TProperty> propertyBuilder,
        string? name = null,
        string? schema = null) => SqlStringHelper.DatabaseProvider switch
        {
#if HAS_SQLSERVER
            SqlStringHelper.SqlServer => SqlServerPropertyBuilderExtensions.UseHiLo(propertyBuilder, name, schema),
#endif
            SqlStringHelper.PostgreSQL => NpgsqlPropertyBuilderExtensions.UseHiLo(propertyBuilder, name, schema),
            _ => throw ThrowHelper.GetArgumentOutOfRangeException(SqlStringHelper.DatabaseProvider),
        };
}
