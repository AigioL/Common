using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserMembershipModel
{
    /// <summary>
    /// 首次成为会员时间
    /// </summary>
    public DateTimeOffset FirstMembershipDate { get; set; }

    /// <summary>
    /// 会员开始时间
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// 会员到期时间
    /// </summary>
    public DateTimeOffset ExpireDate { get; set; }

    /// <summary>
    /// 会员订阅类型，跟随变更记录更新
    /// </summary>
    public MembershipLicenseFlags MemberLicenseFlags { get; set; }

    /// <summary>
    /// 按量付费的会员时长
    /// </summary>
    public TimeSpan PayAsYoGo { get; set; }
}
