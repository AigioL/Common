using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 账号密码登录接口请求模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class AccountLoginRequest : IDeviceId
{
    /// <summary>
    /// 用户名/手机号码/邮箱
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? Password { get; set; }

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
