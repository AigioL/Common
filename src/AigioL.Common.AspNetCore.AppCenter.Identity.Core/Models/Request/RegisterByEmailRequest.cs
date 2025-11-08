using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 账号注册（通过邮箱）接口请求模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class RegisterByEmailRequest : IDeviceId
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? Email { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string Password { get; set; } = string.Empty;

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? Code { get; set; }

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