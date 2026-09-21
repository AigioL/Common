using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AdminCenter.Models.Users;

/// <summary>
/// 设置用户按量付费时长模型类
/// </summary>
public sealed partial class SetUserPayAsYoGoModel : INote
{
#if USE_NUM_UID
    public long UserId { get; set; }
#else
    public Guid UserId { get; set; }
#endif

    /// <summary>
    /// 按量付费时长的目标值，与 <see cref="TimeSpan"/> 二选一
    /// </summary>
    public TimeSpan? PayAsYoGo { get; set; }

    /// <summary>
    /// 按量付费时长的增量值（正数增加、负数减少），与 <see cref="PayAsYoGo"/> 二选一
    /// </summary>
    public TimeSpan? TimeSpan { get; set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string? Note { get; set; }
}
