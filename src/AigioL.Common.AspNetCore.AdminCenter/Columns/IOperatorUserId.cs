using AigioL.Common.Primitives.Columns;

/// <summary>
/// 最后操作人（记录最后操作此条目的管理后台用户）
/// </summary>
public interface IOperatorUserId : IReadOnlyOperatorUserId
{
    /// <inheritdoc cref="IOperatorUserId"/>
    new Guid? OperatorUserId { get; set; }
}

/// <inheritdoc cref="IOperatorUserId"/>
public interface IReadOnlyOperatorUserId
{
    /// <inheritdoc cref="IOperatorUserId"/>
    Guid? OperatorUserId { get; }
}
