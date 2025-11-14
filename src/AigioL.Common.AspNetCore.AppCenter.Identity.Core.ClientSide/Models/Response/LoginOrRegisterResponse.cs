using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;

/// <summary>
/// 登录或注册响应模型，使用 VersionTolerant 以支持向后兼容
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class LoginOrRegisterResponse : IExplicitHasValue
{
    /// <inheritdoc cref="JsonWebTokenValue"/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public JsonWebTokenValue? AuthToken { get; set; }

    /// <summary>
    /// 当前登录的用户信息
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public UserInfoModel? User { get; set; }

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
[Obsolete("use LoginOrRegisterResponseV0", true)]
public sealed class RcLoginOrRegisterResponseCompat { }
#endif