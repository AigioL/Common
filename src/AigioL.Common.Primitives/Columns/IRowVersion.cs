namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 并发令牌
/// <para>https://learn.microsoft.com/zh-cn/ef/core/modeling/concurrency?tabs=data-annotations#timestamprowversion</para>
/// <para>https://www.npgsql.org/efcore/modeling/concurrency.html?tabs=data-annotations</para>
/// </summary>
public interface IRowVersion
{
    /// <inheritdoc cref="IRowVersion"/>
    uint RowVersion { get; set; }
}
