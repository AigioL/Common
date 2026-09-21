using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

/// <summary>
/// 用户会员变动记录模型
/// </summary>
public sealed partial record class UserMembershipChangeRecordModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

#if !USE_NUM_UID
    public Guid UserId { get; set; }
#else
    public long UserId { get; set; }
#endif

    /// <summary>
    /// 变更方向
    /// </summary>
    public MembershipChangeDirection MembershipChangeDirection { get; set; }

    /// <summary>
    /// 会员订阅类型
    /// </summary>
    public MembershipLicenseFlags MemberLicenseType { get; set; }

    /// <summary>
    /// 会员时长变更值
    /// </summary>
    public TimeSpan Value { get; set; }

    /// <summary>
    /// 按量付费的会员时长变更值
    /// </summary>
    public TimeSpan PayAsYoGo { get; set; }

    /// <summary>
    /// 是否为按量付费的时长
    /// </summary>
    public bool IsPayAsYoGo { get; set; }

    /// <summary>
    /// 变更后的实际到期时间
    /// </summary>
    public DateTimeOffset CurrentRealExpireDate { get; set; }

    /// <summary>
    /// 绑定的合作伙伴用户 Id
    /// </summary>
    public Guid? BindPCUserId { get; set; }

    /// <summary>
    /// 绑定的合作伙伴用户到期时间
    /// </summary>
    public DateTimeOffset BindPCUserExpireDate { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreateUser { get; set; }
}
