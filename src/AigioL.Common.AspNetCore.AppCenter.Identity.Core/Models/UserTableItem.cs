using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserTableItem : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    public UserType UserType { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    public string? PersonalizedSignature { get; set; }

    /// <summary>
    /// 经验值
    /// </summary>
    public long Experience { get; set; }

    /// <summary>
    /// 头像 Url
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public DateTimeOffset? BirthDate { get; set; }

    /// <summary>
    /// 出生日期纪录的客户端所在时区
    /// </summary>
    public sbyte BirthDateTimeZone { get; set; }

    /// <summary>
    /// 地区
    /// </summary>
    public int? AreaId { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string? PhoneNumber { get; set; }

    public string? PhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 上次登录时间
    /// </summary>
    public DateTimeOffset LastLoginTime { get; set; }

    /// <summary>
    /// 最后读取官方消息时间
    /// </summary>
    public DateTimeOffset? LastReadSystemMessageTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 修改人 Id
    /// </summary>
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 是否封禁
    /// </summary>
    /// <remarks>
    /// 是否处于封禁状态，判断依据为：封禁结束时间大于当前时间
    /// </remarks>
    public bool IsLockout => LockoutEnd > DateTimeOffset.Now;

    public DateTimeOffset? LockoutEnd { get; set; }
}
