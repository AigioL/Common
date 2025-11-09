using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 换绑手机号码（安全验证）请求模型，使用 VersionTolerant 以支持向后兼容
/// <list type="bullet">
/// <item>【Lable 当前用户的手机号 中间四位隐藏】</item>
/// <item>【TextBox 输入短信验证码】【Button 获取验证码】</item>
/// <item>【TextBox 输入新的手机号码】</item>
/// <item>【Button 提交】</item>
/// </list>
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class ChangePhoneNumberValidationRequest : IReadOnlySmsCode, IReadOnlyPhoneNumber
{
    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? SmsCode { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? PhoneNumberRegionCode { get; set; }
}
