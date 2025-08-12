using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.Primitives.Models;

/// <summary>
/// 用于 Select/ComboBox 的数据源模型
/// </summary>
#if ENABLE_MP
[global::MessagePack.MessagePackObject]
#endif
#if !DISABLE_MP2
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
#endif
public partial class SelectItemModel : ITitle, IDisable
{
    /// <summary>
    /// 标题
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(0)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(0)]
#endif
    public string? Title { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(LastMKeyIndex)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(LastMKeyIndex)]
#endif
    public bool Disable { get; set; }

    /// <summary>
    /// 数据源的数量
    /// </summary>
    public const int Count = 100;

    /// <summary>
    /// 最后一个 MessagePack 序列化 下标，继承自此类，新增需要序列化的字段/属性，标记此值+1，+2
    /// </summary>
    protected const int LastMKeyIndex = 1;
}

/// <summary>
/// 用于 Select/ComboBox 的泛型数据源模型
/// </summary>
/// <typeparam name="T"></typeparam>
#if ENABLE_MP
[global::MessagePack.MessagePackObject]
#endif
#if !DISABLE_MP2
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
#endif
public partial class SelectItemModel<T> : SelectItemModel
{
    /// <summary>
    /// 数据源项的 Id
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(LastMKeyIndex + 1)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(LastMKeyIndex + 1)]
#endif
    public T? Id { get; set; }

    //public static readonly SelectItemDTO<T> Null = new()
    //{
    //    Id = default,
    //    Title = SharedStrings.All,
    //};

    //public string ToDisableStatus()
    //{
    //    var equalityComparer = EqualityComparer<T>.Default;
    //    if (equalityComparer.Equals(Id, default) || (IsEnumOrInteger(typeof(T)) && equalityComparer.Equals(Id, ConvertibleHelper.Convert<T, sbyte>(sbyte.MinValue)))) return "default";
    //    if (Disable) return "error";
    //    return "success";

    //    static bool IsEnumOrInteger(Type type) => type.IsEnum || Type.GetTypeCode(type) switch
    //    {
    //        TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 => true,
    //        _ => false,
    //    };
    //}
}
