namespace AigioL.Common.AspNetCore.AdminCenter.Columns;

/// <summary>
/// 创建人（创建此条目的管理后台用户）
/// </summary>
public interface ICreateUserId : IReadOnlyCreateUserId
{
    /// <inheritdoc cref="ICreateUserId"/>
    new Guid CreateUserId { get; set; }
}

/// <inheritdoc cref="ICreateUserId"/>
public interface IReadOnlyCreateUserId
{
    /// <inheritdoc cref="ICreateUserId"/>
    Guid CreateUserId { get; }
}
