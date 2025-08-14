using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;

namespace AigioL.Common.EntityFrameworkCore.Helpers;

/// <summary>
/// 实体类的列相关的助手类
/// </summary>
public static partial class ColumnHelper
{
    /// <summary>
    /// 类型 <see cref="ISort"/> 的 Type 对象
    /// </summary>
    public static readonly Type PSort = typeof(ISort);

    /// <summary>
    /// 类型 <see cref="ISoftDeleted"/> 的 Type 对象
    /// </summary>
    public static readonly Type PSoftDeleted = typeof(ISoftDeleted);

    /// <summary>
    /// 类型 <see cref="ICreationTime"/> 的 Type 对象
    /// </summary>
    public static readonly Type PCreationTime = typeof(ICreationTime);

    /// <summary>
    /// 类型 <see cref="IUpdateTime"/> 的 Type 对象
    /// </summary>
    public static readonly Type PUpdateTime = typeof(IUpdateTime);

    /// <summary>
    /// 类型 <see cref="IDisable"/> 的 Type 对象
    /// </summary>
    public static readonly Type PDisable = typeof(IDisable);

    /// <summary>
    /// 类型 <see cref="INEWSEQUENTIALID"/> 的 Type 对象
    /// </summary>
    public static readonly Type PNEWSEQUENTIALID = typeof(INEWSEQUENTIALID);

    /// <summary>
    /// 类型 <see cref="IPhoneNumber"/> 的 Type 对象
    /// </summary>
    public static readonly Type PPhoneNumber = typeof(IPhoneNumber);

    /// <summary>
    /// 共享类型 Type 对象
    /// <para>https://docs.microsoft.com/zh-cn/ef/core/modeling/shadow-properties#property-bag-entity-types</para>
    /// </summary>
    public static readonly Type SharedType = typeof(Dictionary<string, object>);

    /// <summary>
    /// 类型 <see cref="IDoNothingEntityGuid"/> 的 Type 对象
    /// </summary>
    public static readonly Type PDoNothingEntityGuid = typeof(IDoNothingEntityGuid);

    public static readonly Type PEntityGuid = typeof(IEntity<Guid>);

    /// <summary>
    /// 类型 <see cref="IRowVersion"/> 的 Type 对象
    /// </summary>
    public static readonly Type PRowVersion = typeof(IRowVersion);

    /// <summary>
    /// 存储了特定类型的对象，这些对象被标记为"软删除"
    /// </summary>
    internal static readonly HashSet<Type> SoftDeleted = [];

    /// <summary>
    /// 判断指定类型是否实现了软删除接口
    /// </summary>
    public static bool IsSoftDeleted(Type type) => SoftDeleted.Contains(type);
}
