using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserEdit : IReadOnlyId<Guid>
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
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// 出生日期纪录的客户端所在时区
    /// </summary>
    public sbyte BirthDateTimeZone { get; set; }

    /// <summary>
    /// 地区
    /// </summary>
    public int? AreaId { get; set; }

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

    /*
        /// <summary>
        /// 修改人 Id
        /// </summary>
        public Guid? OperatorUserId { get; set; }

        /// <summary>
        /// 修改人名称
        /// </summary>
        public string? OperatorUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTimeOffset UpdateTime { get; set; }
    */

    public string? PhoneNumberRegionCode { get; set; }

    #region 来自 IdentityUser

    /// <inheritdoc cref="IdentityUser{T}.PhoneNumber"/>
    public string? PhoneNumber { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.PhoneNumberConfirmed"/>
    public bool PhoneNumberConfirmed { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.TwoFactorEnabled"/>
    public bool TwoFactorEnabled { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.UserName"/>
    public string? UserName { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.Email"/>
    public string? Email { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.EmailConfirmed"/>
    public bool EmailConfirmed { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.LockoutEnd"/>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.LockoutEnabled"/>
    public bool LockoutEnabled { get; set; }

    /// <inheritdoc cref="IdentityUser{T}.AccessFailedCount"/>
    public int AccessFailedCount { get; set; }

    #endregion 来自 IdentityUser

    /// <summary>
    /// 是否封禁
    /// </summary>
    /// <remarks>
    /// 是否处于封禁状态，判断依据为：封禁结束时间大于当前时间
    /// </remarks>
    public bool IsLockout => LockoutEnd > DateTimeOffset.Now;
}
