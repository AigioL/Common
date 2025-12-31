using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

/// <summary>
/// 用户信息模型，使用 VersionTolerant 以支持向后兼容
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class UserInfoModel : IPhoneNumber, IId<Guid>
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? NickName { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public uint Experience { get; set; }

    /// <summary>
    /// 账号余额
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public decimal Balance { get; set; }

    /// <summary>
    /// 账号等级
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public byte Level { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public Gender Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public DateTimeOffset? BirthDate { get; set; }

    /// <summary>
    /// 出生日期时区
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(7)]
    public sbyte BirthDateTimeZone { get; set; }

    /// <summary>
    /// 计算后的年龄
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(8)]
    public byte? CalcAge { get; set; }

    /// <summary>
    /// 所在地，地区 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(9)]
    public int? AreaId { get; set; }

    /// <summary>
    /// 关联的 Apple 账号邮箱（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(10)]
    public string? AppleAccountEmail { get; set; }

    /// <summary>
    /// 头像 Url
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(11)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(12)]
    public UserType UserType { get; set; }

    /// <summary>
    /// 下一级所需经验值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(13)]
    public uint NextExperience { get; set; }

    /// <summary>
    /// 是否签到
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(14)]
    public bool IsSignIn { get; set; }

    [global::MemoryPack.MemoryPackOrder(15)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(16)]
    public string? PersonalizedSignature { get; set; }

    /// <summary>
    /// 第三方业务的 OpenId
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(17)]
    public string? ThirdPartyOpenId { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(18)]
    public string? Email { get; set; }

    /// <summary>
    /// 邮箱地址已确认
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(19)]
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// 已设置密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(20)]
    public bool HasPassword { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(21)]
    public string? PhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 会员信息
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(22)]
    public MembershipInfo? MembershipInfo { get; set; }

    /// <summary>
    /// 绑定的第三方账号
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(23)]
    public List<UserInfoExternalAccountModel>? ExternalAccounts { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"NickName: {NickName}, Id: {Id}";
}

partial class UserInfoModel : IReadOnlyNickNameWithExternalAccounts
{
    IReadOnlyList<IReadOnlyNickName> IReadOnlyNickNameWithExternalAccounts.ExternalAccounts => ExternalAccounts ?? [];
}

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class UserInfoExternalAccountModel : IReadOnlyNickName
{
    /// <summary>
    /// 第三方账号类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public ExternalLoginChannel Type { get; set; }

    /// <summary>
    /// 第三方账号 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string ExternalAccountId { get; set; } = "";

    /// <summary>
    /// 头像 Url
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? NickName { get; set; }
}

#if DEBUG
[Obsolete("use UserInfoModelV0", true)]
public sealed partial class IdentityUserInfoModelCompat
{
}
#endif