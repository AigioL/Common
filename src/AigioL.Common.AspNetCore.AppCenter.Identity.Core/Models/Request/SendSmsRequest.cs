using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 发送短信验证码请求
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class SendSmsRequest : IReadOnlyPhoneNumber
{
    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc cref="SmsCodeType"/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public SmsCodeType Type { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? PhoneNumberRegionCode { get; }
}