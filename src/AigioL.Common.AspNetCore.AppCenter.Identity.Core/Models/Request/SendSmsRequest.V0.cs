using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 发送短信验证码请求（版本 0）
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class SendSmsRequestV0
{
    /// <inheritdoc cref="IPhoneNumber.PhoneNumber"/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc cref="SmsCodeType"/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public SmsCodeType Type { get; set; }
}

#if DEBUG
[Obsolete("use SendSmsRequestV0", true)]
public sealed class SendSmsRequest { }
#endif