namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;

/// <summary>
/// 活跃用户统计响应
/// </summary>
public record class ActiveUserSumResponse
{
    /// <summary>
    /// 总数据量
    /// </summary>
    public int AllCount { get; set; }

    /// <summary>
    /// 登录数量
    /// </summary>
    public int LoginCount { get; set; }

    /// <summary>
    /// 带设备 Id 数量
    /// </summary>
    public int DeviceIdCount { get; set; }

    /// <summary>
    /// IP 去重后数量
    /// </summary>
    public int IPCount { get; set; }

    /// <summary>
    /// 日期
    /// </summary>
    public DateTimeOffset Time { get; set; }
}
