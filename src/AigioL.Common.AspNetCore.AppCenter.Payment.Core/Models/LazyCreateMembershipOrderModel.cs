namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class LazyCreateMembershipOrderModel
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid UserId { get; set; }

    [global::MemoryPack.MemoryPackOrder(1)]
    public Guid MembershipGoodsId { get; set; }

    [global::MemoryPack.MemoryPackOrder(2)]
    public Guid? ChannelPackageId { get; set; }
}
