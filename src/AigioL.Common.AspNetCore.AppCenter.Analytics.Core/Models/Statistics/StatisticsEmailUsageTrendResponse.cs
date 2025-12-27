namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;

/// <summary>
/// 统计邮件发送趋势响应
/// </summary>
public sealed partial class StatisticsEmailUsageTrendResponse
{
    /// <summary>
    /// 模板主键
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string TemplateName { get; set; } = "";

    /// <summary>
    /// 统计时间
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }

    /// <summary>
    /// 邮件数量
    /// </summary>
    public long Count { get; set; }
}