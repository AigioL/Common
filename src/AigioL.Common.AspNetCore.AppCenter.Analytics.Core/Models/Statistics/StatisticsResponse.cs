using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;

/// <summary>
/// 统计图表 Pie 饼图返回结构
/// </summary>
public sealed record class StatisticsPieResponse
{
    public string Name { get; set; } = string.Empty;

    public long Count { get; set; }
}

/// <summary>
/// 统计图表 Line 折线图返回结构
/// </summary>
public sealed record class StatisticsLineResponse
{
    public long Count { get; set; }

    public DateTimeOffset Time { get; set; }
}

/// <summary>
/// 统计图表-系统分布（活跃用户的）数据的返回结构
/// </summary>
public sealed record class StatisticsChartActiveUserOSResponse
{
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public long Count { get; set; }
}

/// <summary>
/// 基础图形响应模型
/// </summary>
public sealed record class AnalysisResponse
{
    public DateOnly Date { get; set; }

    public string? Name { get; set; }

    public long StatisticsCount { get; set; }
}