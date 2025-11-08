using AigioL.Common.AspNetCore.AppCenter.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;

/// <summary>
/// 会员信息模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class MembershipInfo
{
    /// <summary>
    /// 是否是会员
    /// </summary>
    [global::MemoryPack.MemoryPackIgnore]
    public bool IsMembership => ExpireDate.HasValue && ExpireDate > DateTimeOffset.Now;

    /// <summary>
    /// 会员订阅类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public MembershipLicenseFlags MemberLicenseFlags { get; set; }

    /// <summary>
    /// 会员开始时间 if <see cref="IsMembership"/> is <see langword="false"/>, it was <see langword="null"/>
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// 会员过期时间 if <see cref="IsMembership"/> is <see langword="false"/>, it was <see langword="null"/>
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public DateTimeOffset? ExpireDate { get; set; }

    /// <summary>
    /// 首次成为会员时间，会员过期后依旧展示，为 <see langword="null"/> 表示未成为过会员
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public DateTimeOffset? FirstMembershipDate { get; set; }
}
