namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

/// <summary>
/// 会员 CDKey 兑换请求模型类
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class MembershipCDKeyRequest
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid UserId { get; set; }

    [global::MemoryPack.MemoryPackOrder(1)]
    public required string CDKey { get; set; }

#if DEBUG
    [Obsolete("", true)]
    [global::MemoryPack.MemoryPackIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public Guid? ParsedCDKey => ShortGuid.TryParse(CDKey, out Guid parsedGuid) ? parsedGuid : null;
#endif
}
