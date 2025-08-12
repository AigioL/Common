using AigioL.Common.Primitives.Models.Abstractions;
using System.Diagnostics;

namespace AigioL.Common.Primitives.Models;

/// <inheritdoc cref="IPagedModel"/>
#if ENABLE_MP
[global::MessagePack.MessagePackObject]
#endif
#if !DISABLE_MP2
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
#endif
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public partial class PagedModel<T> : IPagedModel<T>, IReadOnlyPagedModel<T>
{
    string DebuggerDisplay() => $"Current: {Current}, Total: {Total}, Count: {mDataSource?.Count ?? 0}, PageSize: {PageSize}";

    /// <summary>
    /// 数据源数组
    /// </summary>
    IList<T>? mDataSource;

    /// <summary>
    /// 获取或设置数据源数组
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(0)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(0)]
#endif
    public IList<T> DataSource
    {
        get
        {
            mDataSource ??= [];
            return mDataSource;
        }

        set => mDataSource = value;
    }

    /// <summary>
    /// 获取或设置当前页数
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(1)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(1)]
#endif
    public int Current { get; set; } = IPagedModel.DefaultCurrent;

    /// <summary>
    /// 获取或设置分页大小
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(2)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(2)]
#endif
    public int PageSize { get; set; } = IPagedModel.DefaultPageSize;

    /// <summary>
    /// 获取或设置总记录数
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(3)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(3)]
#endif
    public int Total { get; set; }

    /// <inheritdoc/>
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return Total >= 0 && PageSize > 0 && Current > 0;
    }

    /// <summary>
    /// 获取数据源的只读列表
    /// </summary>
    IReadOnlyList<T> IReadOnlyPagedModel<T>.DataSource
    {
        get
        {
            var dataSource = DataSource;
            return dataSource is IReadOnlyList<T> list ? list : [.. dataSource];
        }
    }

    /// <inheritdoc cref="DataSource"/>
    IList<T> IPagedModel<T>.DataSource
    {
        get => DataSource;
        set => DataSource = value is IList<T> list ? list : [.. value];
    }
}