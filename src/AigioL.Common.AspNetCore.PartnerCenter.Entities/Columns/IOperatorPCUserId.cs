namespace AigioL.Common.AspNetCore.PartnerCenter.Columns;

/// <summary>
/// 最后操作人（记录最后操作此条目的合作伙伴后台用户）
/// </summary>
public interface IOperatorPCUserId : IReadOnlyOperatorPCUserId
{
    /// <inheritdoc cref="IOperatorPCUserId"/>
    new Guid? OperatorPCUserId { get; set; }
}

/// <inheritdoc cref="IOperatorPCUserId"/>
public interface IReadOnlyOperatorPCUserId
{
    /// <inheritdoc cref="IOperatorPCUserId"/>
    Guid? OperatorPCUserId { get; }
}
