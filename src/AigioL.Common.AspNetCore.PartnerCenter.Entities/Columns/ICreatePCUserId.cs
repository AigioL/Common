namespace AigioL.Common.AspNetCore.PartnerCenter.Columns;

/// <summary>
/// 创建人（创建此条目的合作伙伴后台用户）
/// </summary>
public interface ICreatePCUserId : IReadOnlyCreatePCUserId
{
    /// <inheritdoc cref="ICreatePCUserId"/>
    new Guid CreatePCUserId { get; set; }
}

/// <inheritdoc cref="ICreatePCUserId"/>
public interface IReadOnlyCreatePCUserId
{
    /// <inheritdoc cref="ICreatePCUserId"/>
    Guid CreatePCUserId { get; }
}
