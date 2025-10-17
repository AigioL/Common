using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;

/// <summary>
/// 客户端版本表实体类
/// </summary>
[Table(nameof(AppVer) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AppVer :
    OperatorBaseEntity<Guid>,
    INEWSEQUENTIALID,
    ISoftDeleted,
    IDisable,
    IReadOnlyAppVer
{
    /// <summary>
    /// 私钥
    /// </summary>
    [Required]
    [Comment("私钥")]
    public required string PrivateKey { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [Required]
    [StringLength(MaxLengths.Version)]
    [Comment("版本号")]
    public required string Version { get; set; }

    /// <summary>
    /// 版本信息
    /// </summary>
    [Comment("版本信息")]
    [StringLength(MaxLengths.InformationalVersion)]
    public string? InformationalVersion { get; set; }

    /// <summary>
    /// 版本说明
    /// </summary>
    [Required]
    [Comment("版本说明")]
    public required string ReleaseNote { get; set; }

    /// <summary>
    /// 是否 Beta 版本
    /// </summary>
    [Comment("是否 Beta 版本")]
    public bool BetaVersion { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [Comment("是否删除")]
    public bool SoftDeleted { get; set; }

    /// <summary>
    /// 客户端构建
    /// </summary>
    public virtual List<AppVerBuild> Builds { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<AppVer>
    {
        /// <inheritdoc/>
        public sealed override void Configure(EntityTypeBuilder<AppVer> builder)
        {
            base.Configure(builder);

            builder.HasIndex(x => x.Version);
            builder.HasAlternateKey(x => x.Version);

            builder
                .HasMany(x => x.Builds)
                .WithOne(x => x.AppVer)
                .HasForeignKey(x => x.AppVerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}