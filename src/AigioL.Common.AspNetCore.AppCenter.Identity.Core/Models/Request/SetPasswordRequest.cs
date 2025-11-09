using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 设置登录密码请求模型，使用 VersionTolerant 以支持向后兼容
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class SetPasswordRequest : IReadOnlySmsCode
{
    /// <summary>
    /// 短信验证码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? SmsCode { get; set; }

    /// <summary>
    /// 当前密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? CurrentPassword { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 确认新密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string Password2 { get; set; } = string.Empty;
}
