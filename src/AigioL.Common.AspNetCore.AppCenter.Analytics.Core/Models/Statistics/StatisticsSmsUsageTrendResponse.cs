using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;

/// <summary>
/// 统计短信使用趋势响应
/// </summary>
public sealed partial class StatisticsSmsUsageTrendResponse
{
    /// <inheritdoc cref="SmsCodeType"/>
    public SmsCodeType RequestType { get; set; }

    /// <inheritdoc cref="AuthMessageRecord.SendIsSuccess"/>
    public bool SendIsSuccess { get; set; }

    /// <summary>
    /// 统计时间
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }

    /// <summary>
    /// 短信数量
    /// </summary>
    public long Count { get; set; }
}