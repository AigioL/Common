using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;

/// <summary>
/// 广告表实体类
/// </summary>
[Table("Advertisements")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class Komaasharu :
    OperatorBaseEntity<Guid>,
    ICreationTime,
    ISoftDeleted,
    IDisable,
    ISort,
    INEWSEQUENTIALID,
    IDescription
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    [Comment("名称")]
    [StringLength(MaxLengths.Name)]
    public required string Name { get; set; }

    /// <inheritdoc/>
    [Comment("描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Required]
    [StringLength(MaxLengths.Url)]
    [Comment("图片地址")]
    public required string Url { get; set; }

    /// <summary>
    /// 跳转地址
    /// </summary>
    [Required]
    [StringLength(MaxLengths.Url)]
    [Comment("跳转地址")]
    public required string JumpUrl { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Comment("开始时间")]
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Comment("结束时间")]
    public DateTimeOffset EndTime { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }

    /// <summary>
    /// 广告类型
    /// </summary>
    [Comment("广告类型")]
    public KomaasharuType Type { get; set; } = KomaasharuType.Banner;

    /// <summary>
    /// 总点击数
    /// </summary>
    [Comment("总点击数")]
    public long TotalClick { get; set; }

    /// <summary>
    /// 总展示数
    /// </summary>
    [Comment("总展示数")]
    public long TotalDisplay { get; set; }

    /// <summary>
    /// 广告方向
    /// </summary>
    [Comment("广告方向")]
    public KomaasharuOrientation Orientation { get; set; } = KomaasharuOrientation.Horizontal;

    /// <inheritdoc/>
    [Comment("排序")]
    public long Sort { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    [Comment("平台")]
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    /// <summary>
    /// 设备类型
    /// </summary>
    [Comment("设备类型")]
    public DeviceIdiom DeviceIdiom { get; set; }

    /// <summary>
    /// 客户端判断这个参数跳转到登录中间页然后跳转到 <see cref="JumpUrl"/>
    /// </summary>
    [Comment("IsAuth")]
    public bool IsAuth { get; set; }

    public virtual List<KomaasharuStatistic> Statistics { get; set; } = null!;

    /// <inheritdoc cref="KomaasharuPersonalized" />
    public virtual KomaasharuPersonalized AdvertisementPersonalized { get; set; } = new();

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<Komaasharu>
    {
        public sealed override void Configure(EntityTypeBuilder<Komaasharu> builder)
        {
            base.Configure(builder);

            builder.HasMany(x => x.Statistics)
                .WithOne(x => x.Komaasharu)
                .HasForeignKey(x => x.KomaasharuId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AdvertisementPersonalized)
                .WithOne(x => x.Advertisement)
                .HasForeignKey<KomaasharuPersonalized>(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}