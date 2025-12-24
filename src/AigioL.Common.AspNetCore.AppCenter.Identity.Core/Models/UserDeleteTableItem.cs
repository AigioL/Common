using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserDeleteTableItem : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    public string? PersonalizedSignature { get; set; }

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
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfoModel? UserInfo { get; set; }

    /// <summary>
    /// 关联的外部账号
    /// </summary>
    public List<ExternalAccountModel>? ExternalAccounts { get; set; }
}
