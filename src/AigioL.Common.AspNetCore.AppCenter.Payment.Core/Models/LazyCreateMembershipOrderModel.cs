namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class LazyCreateMembershipOrderModel
{
    [global::MemoryPack.MemoryPackOrder(0)]
#if USE_NUM_UID
    public long UserId { get; set; }
#else
    public Guid UserId { get; set; }
#endif

    [global::MemoryPack.MemoryPackOrder(1)]
    public Guid MembershipGoodsId { get; set; }

    [global::MemoryPack.MemoryPackOrder(2)]
    public Guid? ChannelPackageId { get; set; }
}
