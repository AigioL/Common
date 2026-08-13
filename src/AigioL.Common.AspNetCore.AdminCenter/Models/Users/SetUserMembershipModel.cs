using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AdminCenter.Models.Users;

/// <summary>
/// 设置用户会员时长模型类
/// </summary>
public sealed partial class SetUserMembershipModel : INote
{
#if USE_NUM_UID
    public long UserId { get; set; }
#else
    public Guid UserId { get; set; }
#endif

    /// <summary>
    /// 会员结束时间
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// 使用时间间隔操作会员到期时间
    /// </summary>
    public TimeSpan? TimeSpan { get; set; }

    /// <summary>
    /// 更变原因
    /// </summary>
    public string? Note { get; set; }
}
