using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;

/// <summary>
/// 文章访问统计表实体类
/// </summary>
[Table("ArticleVisitStatistics")]
public partial class ArticleVisitStatistic :
    Entity<Guid>,
    IReadOnlyIPAddress,
    ICreationTime,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 访问 Url
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Required]
    [Comment("访问 Url")]
    public required string VisitUrl { get; set; }

    /// <summary>
    /// 来源 Url
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("来源 Url")]
    public string? SourceUrl { get; set; }

    /// <inheritdoc/>
    [StringLength(MaxLengths.IPAddress)]
    [Required]
    [Comment("IP 地址")]
    public required string IPAddress { get; set; }

    [StringLength(MaxLengths.Url)]
    public string? UserAgent { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }
}