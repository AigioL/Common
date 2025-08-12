namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 排序
/// </summary>
public interface ISort : IReadOnlySort
{
    /// <inheritdoc cref="ISort"/>
    new long Sort { get; set; }

    /// <summary>
    /// 序列起始位置
    /// </summary>
    const string SequenceStartsAt = "SequenceStartsAt";
}

/// <inheritdoc cref="ISort"/>
public interface IReadOnlySort
{
    /// <inheritdoc cref="ISort"/>
    long Sort { get; }
}
