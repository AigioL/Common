using AigioL.Common.AspNetCore.AppCenter.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
[Obsolete("use ResetPasswordRequestV2")]
public sealed partial class ResetPasswordRequestV0
{
    /// <summary>
    /// 验证码类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public AuthMessageType Type { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? Email { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string OTPCode { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 确认新密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public string Password2 { get; set; } = string.Empty;
}
