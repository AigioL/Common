using AigioL.Common.AspNetCore.AppCenter.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 发送邮箱验证码请求
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class SendEmailCodeRequest
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public required string Email { get; set; }

    /// <inheritdoc cref="SmsCodeType"/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public SmsCodeType Type { get; set; }
}