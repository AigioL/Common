using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 登录或注册请求模型（版本 0 与 版本 1）
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
[Obsolete("use LoginOrRegisterRequestV2")]
public sealed partial record class LoginOrRegisterRequestV0 : IDeviceId, IReadOnlySmsCode
{
    /// <inheritdoc cref="IPhoneNumber.PhoneNumber"/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? SmsCode { get; set; }

    /// <inheritdoc cref="LoginChannel"/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public LoginChannel Channel { get; set; } = LoginChannel.Client;

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public Guid DeviceIdG { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string? DeviceIdR { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(5)]
    public string? DeviceIdN { get; set; }
}

#if DEBUG
[Obsolete("use LoginOrRegisterRequestV0", true)]
public sealed class LoginOrRegisterRequest { }
#endif
