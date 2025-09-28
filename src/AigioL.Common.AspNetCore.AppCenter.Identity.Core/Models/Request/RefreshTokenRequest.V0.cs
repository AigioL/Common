using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 刷新 JWT 请求
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class RefreshTokenRequest_V0 : IDeviceId
{
    /// <summary>
    /// 刷新 Token
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? RefreshToken { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public Guid DeviceIdG { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? DeviceIdR { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? DeviceIdN { get; set; }
}

#if DEBUG
[Obsolete("use RefreshTokenRequest_V0", true)]
public sealed class RefreshTokenRequest { }
#endif