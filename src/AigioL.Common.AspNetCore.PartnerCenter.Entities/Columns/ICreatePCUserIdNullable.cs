namespace AigioL.Common.AspNetCore.PartnerCenter.Columns;

/// <summary>
/// 创建人（创建此条目的合作伙伴后台用户）
/// </summary>
public interface ICreatePCUserIdNullable : IReadOnlyCreatePCUserIdNullable
{
    /// <inheritdoc cref="ICreatePCUserIdNullable"/>
    new Guid? CreatePCUserId { get; set; }
}

/// <inheritdoc cref="ICreatePCUserIdNullable"/>
public interface IReadOnlyCreatePCUserIdNullable
{
    /// <inheritdoc cref="ICreatePCUserIdNullable"/>
    Guid? CreatePCUserId { get; }
}
