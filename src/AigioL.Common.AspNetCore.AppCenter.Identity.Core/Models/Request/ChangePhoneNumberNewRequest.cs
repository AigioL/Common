using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 换绑手机号码（绑定新手机号）请求模型，使用 VersionTolerant 以支持向后兼容
/// <list type="bullet">
/// <item>【TextBox 输入新手机号码的短信验证码】</item>
/// <item>【Button 完成】</item>
/// </list>
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class ChangePhoneNumberNewRequest : IReadOnlySmsCode, IReadOnlyPhoneNumber
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

    /// <summary>
    /// 上一个接口返回的 CODE
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? Code { get; set; }
}
