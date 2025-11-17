using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;

/// <summary>
/// 广告个性化推荐表实体类
/// </summary>
[Table("AdvertisementPersonalizeds")]
public partial class KomaasharuPersonalized :
    IEntity<Guid>,
    ICreationTime
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public virtual Komaasharu Advertisement { get; set; } = null!;

    /// <summary>
    /// 特征码
    /// </summary>
    [Comment("特征码")]
    public string? FeatureCode { get; set; }

    /// <summary>
    /// 广告标签
    /// </summary>
    [Comment("广告标签")]
    public string? Tags { get; set; }

    /// <summary>
    /// 匹配关键字
    /// </summary>
    [Comment("匹配关键字")]
    public string? MatchWords { get; set; }

    /// <summary>
    /// 适合最小年龄
    /// </summary>
    [Comment("适合最小年龄")]
    public byte SuitableMinAge { get; set; } = 18;

    /// <summary>
    /// 适合最大年龄
    /// </summary>
    [Comment("适合最大年龄")]
    public byte SuitableMaxAge { get; set; } = MaxLengths.HumanAge;

    /// <summary>
    /// 适合性别
    /// </summary>
    [Comment("适合性别")]
    public Gender SuitableGender { get; set; }

    /// <summary>
    /// 适合地理位置
    /// </summary>
    [Comment("适合地理位置")]
    public string? SuitableLocation { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }
}