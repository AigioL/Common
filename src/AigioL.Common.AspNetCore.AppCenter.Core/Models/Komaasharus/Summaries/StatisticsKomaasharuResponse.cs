namespace AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;

/// <summary>
/// 广告统计响应模型类
/// </summary>
public record class StatisticsKomaasharuResponse
{
    /// <summary>
    /// 展示数
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// 点击数
    /// </summary>
    public long ClickCount { get; set; }

    /// <summary>
    /// 统计时间
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }
}
