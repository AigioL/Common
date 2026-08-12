namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 昵称
/// </summary>
public interface INickName : IReadOnlyNickName
{
    /// <inheritdoc cref="INickName"/>
    new string? NickName { get; set; }
}

public interface IReadOnlyNickName
{
    /// <inheritdoc cref="INickName"/>
    string? NickName { get; }
}

public partial interface IReadOnlyNickNameWithExternalAccounts : IReadOnlyNickName
{
    IReadOnlyList<IReadOnlyNickName> ExternalAccounts { get; }
}

#if !USE_NUM_UID
partial interface IReadOnlyNickNameWithExternalAccounts : IReadOnlyId<Guid>;
#else
partial interface IReadOnlyNickNameWithExternalAccounts : IReadOnlyId<long>;
#endif