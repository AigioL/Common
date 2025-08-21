using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;

public record class UserActivityStatisticsResponse
{
    /// <summary>
    /// 平台
    /// </summary>
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    public DeviceIdiom DeviceIdiom { get; set; }

    /// <summary>
    /// 活跃度类型
    /// </summary>
    public ActiveUserStatisticsType AUType { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 统计日期 （当天的数据）
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }
}