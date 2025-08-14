using AigioL.Common.EntityFrameworkCore.Helpers;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static AigioL.Common.EntityFrameworkCore.Helpers.ColumnHelper;
using static AigioL.Common.EntityFrameworkCore.Helpers.SqlStringHelper;

namespace AigioL.Common.EntityFrameworkCore.Extensions;

public static partial class ModelBuilderExtensions
{
    /// <summary>
    /// 软删除的查询过滤表达式
    /// <para>https://docs.microsoft.com/zh-cn/ef/core/querying/filters#example</para>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "MakeGenericType is safe because IEntity<TPrimaryKey>.LambdaEqualId is a reference type.")]
    static LambdaExpression SoftDeletedQueryFilter([DynamicallyAccessedMembers(IEntity.DAMT)] Type type)
    {
        var parameter = Expression.Parameter(type);
        var left = Expression.Property(parameter, type, nameof(ISoftDeleted.SoftDeleted));
        var body = Expression.Not(left);
        return Expression.Lambda(typeof(Func<,>).MakeGenericType(type, typeof(bool)), body, parameter);
    }

    /// <summary>
    /// 根据实体模型继承的接口，生成列的 索引/默认值
    /// 在 <see cref="DbContext.OnModelCreating(ModelBuilder)"/> 中调用此函数，仅支持 SqlServer
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="action"></param>
    public static IEnumerable<IMutableEntityType> BuildEntities(this ModelBuilder modelBuilder, Func<ModelBuilder, IMutableEntityType, Type, Action<EntityTypeBuilder>?, Action<EntityTypeBuilder>?>? action = null)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();
        if (entityTypes == null) throw new NullReferenceException(nameof(entityTypes));
        foreach (var entityType in entityTypes)
        {
            var type = entityType.ClrType;
            if (type == SharedType) continue;

            Action<EntityTypeBuilder>? buildAction = null;

            #region 继承自 排序(IOrder) 接口的要设置索引

            if (PSort.IsAssignableFrom(type))
            {
                // https://docs.microsoft.com/zh-cn/ef/core/modeling/sequences
                var sequenceOrderNumbers = $"{entityType.GetTableName() ?? type.Name}_OrderNumbers"; // 序列名称固定以兼容旧结构
                if (entityType.ClrType.GetProperty(ISort.SequenceStartsAt, BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is not long startValue) startValue = 1L;
                modelBuilder.HasSequence(sequenceOrderNumbers)
                    .StartsAt(startValue)
                    .IncrementsBy(1);

                buildAction += p =>
                {
                    p.Property(nameof(ISort.Sort))
                        .HasDefaultValueSql(
                        NextValueSequenceDefaultValueSql(sequenceOrderNumbers));
                    p.HasIndex(nameof(ISort.Sort));
                };
            }

            #endregion

            #region 继承自 软删除(IsSoftDeleted) 接口的要设置索引

            if (PSoftDeleted.IsAssignableFrom(type))
            {
                // https://docs.microsoft.com/zh-cn/ef/core/querying/filters
                buildAction += p =>
                {
                    p.HasIndex(nameof(ISoftDeleted.SoftDeleted));
                    p.HasQueryFilter(SoftDeletedQueryFilter(type));
                    SoftDeleted.Add(type);
                };
            }

            #endregion

            #region 继承自 创建时间(ICreationTime) 接口的要设置默认值使用数据库当前时间

            if (PCreationTime.IsAssignableFrom(type))
            {
                buildAction += p =>
                {
                    p.Property(nameof(ICreationTime.CreationTime)).HasDefaultValueSql(DateTimeOffsetDefaultValueSql).IsRequired();
                };
            }

            #endregion

            #region 继承自 更新时间(IUpdateTime) 接口的要设置默认值使用数据库当前时间与更新时间

            if (PUpdateTime.IsAssignableFrom(type))
            {
                buildAction += p =>
                {
                    p.Property(nameof(IUpdateTime.UpdateTime)).HasDefaultValueSql(DateTimeOffsetDefaultValueSql).IsRequired();
                    p.Property(nameof(IUpdateTime.UpdateTime)).ValueGeneratedOnAddOrUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                };
            }

            #endregion

            #region 继承自 手机号码(IPhoneNumber) 接口的要设置最大长度

            if (PPhoneNumber.IsAssignableFrom(type))
            {
                buildAction += p =>
                {
                    p.Property(nameof(IPhoneNumber.PhoneNumber)).HasMaxLength(IPhoneNumber.DatabaseMaxLength);
                    p.Property(nameof(IPhoneNumber.PhoneNumberRegionCode)).HasMaxLength(IPhoneNumber.RegionCodeDatabaseMaxLength);
                };
            }

            #endregion

            #region 继承自 主键为 GUID 接口

            if (PDoNothingEntityGuid.IsAssignableFrom(type))
            {
                // 优先级最高
            }
            else if (PNEWSEQUENTIALID.IsAssignableFrom(type))
            {
                buildAction += p =>
                {
                    var pId = p.Property(nameof(INEWSEQUENTIALID.Id));
                    switch (DatabaseProvider)
                    {
                        case SqlServer:
                            pId.HasDefaultValueSql(NEWSEQUENTIALID);
                            break;
                        case PostgreSQL:
                            {
                                if (PostgreSQL18Plus)
                                {
                                    pId.HasDefaultValueSql(uuidv7);
                                }
                                else
                                {
#if NET9_0_OR_GREATER
                                    // https://www.npgsql.org/efcore/modeling/generated-properties.html#guiduuid-generation
                                    // .NET 9.0+ 支持 GUID v7
                                    pId.HasValueGenerator<global::Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.NpgsqlSequentialGuidValueGenerator>();
#else
                                    pId.HasDefaultValueSql(gen_random_uuid);
#endif
                                }
                            }
                            break;
                        default:
                            pId.HasValueGenerator<global::Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.NpgsqlSequentialGuidValueGenerator>();
                            break;
                    }
                };
            }
            else if (PEntityGuid.IsAssignableFrom(type))
            {
                switch (DatabaseProvider)
                {
                    case PostgreSQL: // 默认使用 GUID v7
                        {
                            buildAction += p =>
                            {
                                var pId = p.Property(nameof(IEntity<>.Id));
                                if (PostgreSQL18Plus)
                                {
                                    pId.HasDefaultValueSql(uuidv7);
                                }
                                else
                                {
#if NET9_0_OR_GREATER
                                    // https://www.npgsql.org/efcore/modeling/generated-properties.html#guiduuid-generation
                                    // .NET 9.0+ 支持 GUID v7
                                    pId.HasValueGenerator<global::Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.NpgsqlSequentialGuidValueGenerator>();
#endif
                                }
                            };
                        }
                        break;
                }
            }

            #endregion

            #region 继承自 禁用或启用(IDisable) 接口的要设置默认值为 false

            if (PDisable.IsAssignableFrom(type))
            {
                buildAction += p =>
                {
                    p.Property(nameof(IDisable.Disable)).HasDefaultValue(false);
                };
            }

            #endregion

            switch (DatabaseProvider)
            {
                case PostgreSQL:
                    {
                        if (PRowVersion.IsAssignableFrom(type))
                        {
                            buildAction += p =>
                            {
                                // https://www.npgsql.org/efcore/modeling/concurrency.html?tabs=fluent-api
                                p.Property(nameof(IRowVersion.RowVersion)).IsRowVersion();
                            };
                        }
                    }
                    break;
            }

            if (action != null)
            {
                buildAction = action.Invoke(modelBuilder, entityType, type, buildAction);
            }

            if (buildAction != null)
            {
                modelBuilder.Entity(type, p =>
                {
                    buildAction(p);
                });
            }
        }
        return entityTypes;
    }
}
