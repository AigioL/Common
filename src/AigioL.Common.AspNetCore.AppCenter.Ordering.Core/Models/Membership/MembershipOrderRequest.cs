namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

/// <summary>
/// 创建会员订单请求模型类
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class MembershipOrderRequest
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid UserId { get; set; }

    [global::MemoryPack.MemoryPackOrder(1)]
    public Guid MembershipGoodsId { get; set; }
}