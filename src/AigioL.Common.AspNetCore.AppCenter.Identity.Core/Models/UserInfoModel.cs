using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

/// <summary>
/// 用户信息模型（版本 -1）
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.Object, global::MemoryPack.SerializeLayout.Explicit)]
[Obsolete("use UserInfoModelV0")]
public sealed partial class UserInfoModelV_1
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    [global::MemoryPack.MemoryPackOrder(1)]
    public string NickName { get; set; } = string.Empty;

    [global::MemoryPack.MemoryPackOrder(2)]
    public Guid Avatar { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public uint Experience { get; set; }

    /// <summary>
    /// 机油、体力、疲劳值、积分（类型一）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public int EngineOil { get; set; }

    /// <summary>
    /// 代币、硬币、积分（类型二）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public decimal Coin { get; set; }

    /// <summary>
    /// 余额
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public decimal Balance { get; set; }

    /// <summary>
    /// 账号等级
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(7)]
    public byte Level { get; set; }

    /// <summary>
    /// 关联的 Steam 账号 ID（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(8)]
    public long? SteamAccountId { get; set; }

    [global::MemoryPack.MemoryPackOrder(9)]
    public Gender Gender { get; set; }

    [global::MemoryPack.MemoryPackOrder(10)]
    public DateTime? BirthDate { get; set; }

    [global::MemoryPack.MemoryPackOrder(11)]
    public sbyte BirthDateTimeZone { get; set; }

    /// <summary>
    /// 计算后的年龄
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(12)]
    public byte? CalcAge { get; set; }

    [global::MemoryPack.MemoryPackOrder(13)]
    public int? AreaId { get; set; }

    /// <summary>
    /// 关联的 Microsoft 账号邮箱（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(14)]
    public string? MicrosoftAccountEmail { get; set; }

    [Obsolete("Unable to get the true value, use QQNickName instead.")]
    [global::MemoryPack.MemoryPackOrder(15)]
    public long? QQAccountNumber { get; set; }

    /// <summary>
    /// 关联的 Apple 账号邮箱（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(16)]
    public string? AppleAccountEmail { get; set; }

    /// <summary>
    /// 关联的 QQ 账号昵称（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(17)]
    public string? QQNickName { get; set; }

    /// <summary>
    /// 关联的外部账号头像 Url 列表
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(18)]
    public Dictionary<ExternalLoginChannel, string>? AvatarUrl { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(19)]
    public UserType UserType { get; set; }

    /// <summary>
    /// 下一级所需经验值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(20)]
    public uint NextExperience { get; set; }

    /// <summary>
    /// 是否签到
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(21)]
    public bool IsSignIn { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(22)]
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// 用户信息模型（版本 0）使用 VersionTolerant 以支持向后兼容
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class UserInfoModelV0 : IPhoneNumber
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    [global::MemoryPack.MemoryPackOrder(1)]
    public string NickName { get; set; } = string.Empty;

    [global::MemoryPack.MemoryPackOrder(2)]
    public Guid Avatar { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public uint Experience { get; set; }

    /// <summary>
    /// 余额
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public decimal Balance { get; set; }

    /// <summary>
    /// 账号等级
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public byte Level { get; set; }

    /// <summary>
    /// 关联的 Steam 账号 ID（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public long? SteamAccountId { get; set; }

    [global::MemoryPack.MemoryPackOrder(7)]
    public Gender Gender { get; set; }

    [global::MemoryPack.MemoryPackOrder(8)]
    public DateTime? BirthDate { get; set; }

    [global::MemoryPack.MemoryPackOrder(9)]
    public sbyte BirthDateTimeZone { get; set; }

    /// <summary>
    /// 计算后的年龄
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(10)]
    public byte? CalcAge { get; set; }

    [global::MemoryPack.MemoryPackOrder(11)]
    public int? AreaId { get; set; }

    /// <summary>
    /// 关联的 Microsoft 账号邮箱（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(12)]
    public string? MicrosoftAccountEmail { get; set; }

    /// <summary>
    /// 关联的 Apple 账号邮箱（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(13)]
    public string? AppleAccountEmail { get; set; }

    /// <summary>
    /// 关联的 QQ 账号昵称（如果有）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(14)]
    public string? QQNickName { get; set; }

    /// <summary>
    /// 关联的外部账号头像 Url 列表
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(15)]
    public Dictionary<ExternalLoginChannel, string>? AvatarUrl { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(16)]
    public UserType UserType { get; set; }

    /// <summary>
    /// 下一级所需经验值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(17)]
    public uint NextExperience { get; set; }

    /// <summary>
    /// 是否签到
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(18)]
    public bool IsSignIn { get; set; }

    [global::MemoryPack.MemoryPackOrder(19)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(20)]
    public string? PersonalizedSignature { get; set; }

    /// <summary>
    /// 第三方业务的 OpenId
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(21)]
    public string? ThirdPartyOpenId { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(22)]
    public string? Email { get; set; }

    /// <summary>
    /// 邮箱地址已确认
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(23)]
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// 已设置密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(24)]
    public bool HasPassword { get; set; }

    [global::MemoryPack.MemoryPackOrder(25)]
    public string? PhoneNumberRegionCode { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"NickName: {NickName}, Id: {Id}";
}

#if DEBUG
[Obsolete("use UserInfoModelV_1", true)]
public sealed partial class UserInfoDTOCompat
{
}

[Obsolete("use UserInfoModelV0", true)]
public sealed partial class IdentityUserInfoModelCompat
{
}
#endif