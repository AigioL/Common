using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 登录或注册请求模型，使用 VersionTolerant 以支持向后兼容
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class LoginOrRegisterRequest : IDeviceId, IReadOnlyPhoneNumber, IReadOnlySmsCode
{
    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? SmsCode { get; set; }

    /// <inheritdoc cref="LoginChannel"/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public LoginChannel Channel { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public Guid DeviceIdG { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string? DeviceIdR { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(5)]
    public string? DeviceIdN { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(6)]
    public string? PhoneNumberRegionCode { get; set; }
}
