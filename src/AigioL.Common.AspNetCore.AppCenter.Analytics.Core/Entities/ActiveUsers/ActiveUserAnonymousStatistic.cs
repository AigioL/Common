using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;

/// <summary>
/// 活跃用户匿名统计
/// </summary>
[Table(nameof(ActiveUserAnonymousStatistic) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ActiveUserAnonymousStatistic :
    Entity<Guid>,
    ICreationTime,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 用户活跃类型
    /// </summary>
    [Comment("用户活跃类型")]
    public ActiveUserAnonymousStatisticType Type { get; set; }

    /// <summary>
    /// IP 地址
    /// </summary>
    [Required]
    [Comment("IP 地址")]
    [StringLength(MaxLengths.IPAddress)]
    public required string IPAddress { get; set; }

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
    /// 进程 CPU 架构
    /// </summary>
    [Comment("进程 CPU 架构")]
    public ArchitectureFlags ProcessArch { get; set; }

    [Required] // EF not null
    [Comment("操作系统版本号")]
    public required string OSVersion { get; set; }

    [Comment("App 版本号")]
    [StringLength(MaxLengths.Version)]
    public string? AppVersion { get; set; }

    //public virtual AppVer? AppVersionInfo { get; set; }

    /// <summary>
    /// 屏幕总数
    /// </summary>
    [Comment("屏幕总数")]
    public int ScreenCount { get; set; }

    /// <summary>
    /// 主屏幕像素密度
    /// </summary>
    [Comment("主屏幕像素密度")]
    public double PrimaryScreenPixelDensity { get; set; }

    /// <summary>
    /// 主屏幕宽度
    /// </summary>
    [Comment("主屏幕宽度")]
    public int PrimaryScreenWidth { get; set; }

    /// <summary>
    /// 主屏幕高度
    /// </summary>
    [Comment("主屏幕高度")]
    public int PrimaryScreenHeight { get; set; }

    /// <summary>
    /// 总屏幕宽度
    /// </summary>
    [Comment("总屏幕宽度")]
    public int SumScreenWidth { get; set; }

    /// <summary>
    /// 总屏幕高度
    /// </summary>
    [Comment("总屏幕高度")]
    public int SumScreenHeight { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 是否已登录账号
    /// </summary>
    [Comment("是否已登录账号")]
    public bool? IsAuthenticated { get; set; }

    /// <summary>
    /// 客户端系统平台
    /// </summary>
    [Comment("客户端系统平台")]
    public DevicePlatform2 OSName { get; set; }

    /// <summary>
    /// 设备 Id
    /// </summary>
    [Comment("设备 Id")]
    [StringLength(MaxLengths.DeviceId)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// 客户端版本 Id
    /// </summary>
    [Comment("客户端版本 Id")]
    public Guid? ClientVersionId { get; set; }

    /// <summary>
    /// 客户端分发渠道
    /// </summary>
    [Comment("客户端分发渠道")]
    public int ClientDistributionChannel { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<ActiveUserAnonymousStatistic>
    {
        public void Configure(EntityTypeBuilder<ActiveUserAnonymousStatistic> builder)
        {
            builder.HasIndex(x => x.CreationTime);
        }
    }
}