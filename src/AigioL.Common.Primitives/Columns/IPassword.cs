namespace AigioL.Common.Primitives.Columns;

#if DEBUG
/// <summary>
/// 密码，禁止使用明文字符串存储
/// <para>⚠️ 使用 Microsoft.AspNetCore.Identity.IPasswordHasher 的哈希密码或带密钥加密算法密文（取决于是否需要还原密码明文）</para>
/// </summary>
[Obsolete("use IPasswordHash or IPasswordEncrypted", true)]
public interface IPassword : IReadOnlyPassword
{
    /// <inheritdoc cref="IPassword"/>
    new string? Password { get; set; }
}

/// <inheritdoc cref="IPassword"/>
[Obsolete("use IReadOnlyPasswordHash or IReadOnlyPasswordEncrypted", true)]
public interface IReadOnlyPassword
{
    /// <inheritdoc cref="IPassword"/>
    string? Password { get; }
}
#endif

/// <summary>
/// 使用 Microsoft.AspNetCore.Identity.IPasswordHasher 的哈希密码
/// </summary>
public interface IPasswordHash : IReadOnlyPasswordHash
{
    /// <inheritdoc cref="IPasswordHash"/>
    new string? PasswordHash { get; set; }
}

/// <inheritdoc cref="IPasswordHash"/>
public interface IReadOnlyPasswordHash
{
    /// <inheritdoc cref="IPasswordHash"/>
    string? PasswordHash { get; }
}

/// <summary>
/// 使用密钥加密的密文密码
/// </summary>
public interface IPasswordEncrypted : IReadOnlyPasswordEncrypted
{
    /// <inheritdoc cref="IPasswordEncrypted"/>
    new string? PasswordEncrypted { get; set; }
}

/// <inheritdoc cref="IPasswordHash"/>
public interface IReadOnlyPasswordEncrypted
{
    /// <inheritdoc cref="IPasswordEncrypted"/>
    string? PasswordEncrypted { get; }
}
