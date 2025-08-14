namespace AigioL.Common.AspNetCore.AdminCenter.Columns;

/// <summary>
/// 创建人（创建此条目的管理后台用户）
/// </summary>
public interface ICreateUserIdNullable : IReadOnlyCreateUserIdNullable
{
    /// <inheritdoc cref="ICreateUserIdNullable"/>
    new Guid? CreateUserId { get; set; }
}

/// <inheritdoc cref="ICreateUserIdNullable"/>
public interface IReadOnlyCreateUserIdNullable
{
    /// <inheritdoc cref="ICreateUserIdNullable"/>
    Guid? CreateUserId { get; }
}
