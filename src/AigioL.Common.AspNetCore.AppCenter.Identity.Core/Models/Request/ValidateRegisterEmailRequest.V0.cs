namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
[Obsolete("use ValidateRegisterEmailRequestV2")]
public sealed partial class ValidateRegisterEmailRequestV0
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public required string Email { get; set; }
}