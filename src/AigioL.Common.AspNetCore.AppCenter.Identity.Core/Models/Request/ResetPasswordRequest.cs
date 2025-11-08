using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class ResetPasswordRequest : IPhoneNumber
{
    /// <summary>
    /// 验证码类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public AuthMessageType Type { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮件地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? Email { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? OTPCode { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string? Password { get; set; }

    /// <summary>
    /// 确认新密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public string? Password2 { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(6)]
    public string? PhoneNumberRegionCode { get; set; }
}
