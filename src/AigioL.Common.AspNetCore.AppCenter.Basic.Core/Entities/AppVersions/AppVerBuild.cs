using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;

/// <summary>
/// 客户端版本构建表实体类
/// </summary>
[Table(nameof(AppVerBuild) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AppVerBuild :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreationTime,
    IDisable
{
    /// <summary>
    /// 版本 Id
    /// </summary>
    [Required]
    [Comment("版本 Id")]
    public Guid AppVerId { get; set; }

    /// <summary>
    /// 分发渠道
    /// </summary>
    [Comment("分发渠道")]
    public ClientDistributionChannel ClientDistributionChannel { get; set; }

    /*
        /// <summary>
        /// (单选)平台
        /// </summary>
        [Comment("平台")]
        public Platform Platform { get; set; }

        /// <summary>
        /// (单选)设备种类
        /// </summary>
        [Comment("设备种类")]
        public DeviceIdiom DeviceIdiom { get; set; }

        /// <summary>
        /// (多或单选)支持的 CPU 架构
        /// </summary>
        [Comment("支持的 CPU 架构")]
        public ArchitectureFlags SupportedAbis { get; set; }
    */

    /// <summary>
    /// 客户端设备
    /// </summary>
    [Comment("客户端设备")]
    public ClientPlatform ClientDeviceType { get; set; }

    /// <summary>
    /// (单选)应用程序部署模式
    /// </summary>
    [Comment("应用程序部署模式")]
    public DeploymentMode DeploymentMode { get; set; }

    /// <summary>
    /// 可更新时间
    /// </summary>
    [Comment("可更新时间")]
    public DateTimeOffset? AvailableTime { get; set; }

    [Comment("是否禁用")]
    public bool Disable { get; set; }

    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    public virtual AppVer AppVer { get; set; } = null!;

    /// <summary>
    /// 客户端文件
    /// </summary>
    public virtual List<AppVerFile> Files { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<AppVerBuild>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<AppVerBuild> builder)
        {
            //builder.HasIndex(x => new { x.Platform, x.DeviceIdiom, x.SupportedAbis });

            builder
                .HasMany(x => x.Files)
                .WithOne(x => x.Build)
                .HasForeignKey(x => x.AppVerBuildId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public static ClientDistributionChannel GetClientDistributionChannel(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DeviceIdiom deviceIdiom,
        bool isRunningAsUwp)
    {
        var distributionChannel = ClientDistributionChannel.Official;
        switch (platform)
        {
#pragma warning disable CS0612 // 类型或成员已过时
#pragma warning disable CS0618 // 类型或成员已过时
            case WebApiCompatDevicePlatform.Unknown:
                break;
            case WebApiCompatDevicePlatform.Windows:
            case WebApiCompatDevicePlatform.UWP:
            case WebApiCompatDevicePlatform.WinUI:
                if (isRunningAsUwp)
                    distributionChannel = ClientDistributionChannel.MicrosoftStore;
                break;
            case WebApiCompatDevicePlatform.Linux:
                break;
            case WebApiCompatDevicePlatform.Android:
                break;
            case WebApiCompatDevicePlatform.Apple:
                switch (deviceIdiom)
                {
                    case DeviceIdiom.Desktop:
                        if (isRunningAsUwp)
                            distributionChannel = ClientDistributionChannel.AppleAppStore;
                        break;
                    case DeviceIdiom.Phone:
                    case DeviceIdiom.Tablet:
                    case DeviceIdiom.TV:
                    case DeviceIdiom.Watch:
                        distributionChannel = ClientDistributionChannel.AppleAppStore;
                        break;
                }
                break;
#pragma warning restore CS0612 // 类型或成员已过时
#pragma warning restore CS0618 // 类型或成员已过时
            default:
                break;
        }
        return distributionChannel;
    }

    public static ClientDistributionChannel GetClientDistributionChannel(ClientPlatform platform) => platform switch
    {
        // 暂时没有 Apple 应用商店版本，所以 macOS 也走 Official
        //ClientPlatform.macOSX64 or
        //ClientPlatform.macOSArm64 or
        ClientPlatform.iOSArm64 or
        ClientPlatform.iPadOSArm64 or
        ClientPlatform.watchOSArm64 or
        ClientPlatform.tvOSArm64 => ClientDistributionChannel.AppleAppStore,

        ClientPlatform.Win32StoreX86 or
        ClientPlatform.Win32StoreX64 or
        ClientPlatform.Win32StoreArm64 => ClientDistributionChannel.MicrosoftStore,

        _ => ClientDistributionChannel.Official,
    };
}