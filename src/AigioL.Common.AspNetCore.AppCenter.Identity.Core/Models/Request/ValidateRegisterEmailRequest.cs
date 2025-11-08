namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class ValidateRegisterEmailRequest
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public required string Email { get; set; }
}
