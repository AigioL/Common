using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;

/// <summary>
/// 登录或注册响应模型（版本 -1）
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
[Obsolete("use LoginOrRegisterResponseV1")]
public sealed partial record class LoginOrRegisterResponseV_1 : IExplicitHasValue
{
    /// <inheritdoc cref="JsonWebTokenValue"/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public JsonWebTokenValue? AuthToken { get; set; }

    /// <summary>
    /// 当前登录的用户信息
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public UserInfoModelV_1? User { get; set; }

    /// <summary>
    /// 当前操作是登录(<see langword="true"/>)还是注册(<see langword="false"/>)
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public bool IsLoginOrRegister { get; set; }

    /// <inheritdoc cref="IPhoneNumber.PhoneNumber"/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc cref="ExternalLoginChannel"/>
    [global::MemoryPack.MemoryPackOrder(4)]
    public ExternalLoginChannel? FastLRBChannel { get; set; }

    bool IExplicitHasValue.ExplicitHasValue()
    {
        var hasToken = AuthToken != null;
        if (IsLoginOrRegister)
        {
            return hasToken;
        }
        else
        {
            return hasToken && User != null;
        }
    }
}

#if DEBUG
[Obsolete("use LoginOrRegisterResponseV_1", true)]
public sealed class LoginOrRegisterResponseCompat { }
#endif