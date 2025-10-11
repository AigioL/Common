namespace AigioL.Common.JsonWebTokens.Models;

/// <summary>
/// Json Web Token 值
/// </summary>
#if ENABLE_MP
[global::MessagePack.MessagePackObject]
#endif
#if !DISABLE_MP2
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
#endif
public sealed partial record class JsonWebTokenValue : IExplicitHasValue
{
    /// <summary>
    /// 凭证有效期
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(0)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(0)]
#endif
    public DateTimeOffset ExpiresIn { get; set; }

    /// <summary>
    /// 当前凭证
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(1)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(1)]
#endif
    public string? AccessToken { get; set; }

    /// <summary>
    /// 刷新凭证
    /// </summary>
#if ENABLE_MP
    [global::MessagePack.Key(2)]
#endif
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(2)]
#endif
    public string? RefreshToken { get; set; }

    /// <inheritdoc/>
    bool IExplicitHasValue.ExplicitHasValue()
    {
        // 仅数据格式是否正确，不验证时间有效期等业务逻辑
        return !string.IsNullOrEmpty(AccessToken);
    }
}

#if DEBUG
[Obsolete("use JsonWebTokenValue", true)]
public sealed record class JWTEntity { }
#endif