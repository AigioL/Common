using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 登录或注册请求模型（版本 -1）
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class LoginOrRegisterRequest_V_1 : IReadOnlySmsCode
{
    /// <inheritdoc cref="IPhoneNumber.PhoneNumber"/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? SmsCode { get; set; }

    /// <inheritdoc cref="LoginChannel"/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public LoginChannel Channel { get; set; }
}

#if DEBUG
[Obsolete("use LoginOrRegisterRequest_V_1", true)]
public sealed class LoginOrRegisterRequestCompat { }
#endif