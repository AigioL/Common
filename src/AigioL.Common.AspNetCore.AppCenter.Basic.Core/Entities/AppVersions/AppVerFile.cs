using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;

/// <summary>
/// 客户端文件表实体类
/// </summary>
[Table(nameof(AppVerFile) + "s")]
[Index(nameof(AppVerBuildId), nameof(CreationTime))]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AppVerFile :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreationTime,
    ICloudFileInfo
{
    /// <summary>
    /// 客户端构建 Id
    /// </summary>
    [Required]
    [Comment("客户端构建 Id")]
    public Guid AppVerBuildId { get; set; }

    /// <summary>
    /// 静态资源 Id
    /// </summary>
    [Comment("静态资源 Id")]
    public Guid StaticResourceId { get; set; }

    /// <summary>
    /// 适用版本 Id
    /// </summary>
    /// <remarks>
    /// 表示可通过此包更新的版本；
    /// 如果未指定，则表示此包为全量更新包，所有版本可使用此包更新。
    /// </remarks>
    [Comment("适用版本 Id")]
    public Guid? ApplicableAppVerId { get; set; }

    ///// <summary>
    ///// 需要 .NET 运行时版本 Id
    ///// </summary>
    ///// <remarks>
    ///// 表示此包需要的 .NET 运行时版本；如果未指定，则表示更新到此包无需升级 .NET 运行时版本
    ///// </remarks>
    //[Comment("需要 .NET 运行时版本 Id")]
    //public Guid? RequiredDotNetVersionId { get; set; }

    #region CloudFileInfo

    /// <inheritdoc/>
    [StringLength(MaxLengths.FileName)]
    [Comment("文件名")]
    public string? FileName { get; set; }

    /// <summary>
    /// 文件指纹 SHA256
    /// </summary>
    [StringLength(MaxLengths.SHA256 + 3, MinimumLength = MaxLengths.SHA256)]
    public string? SHA256 { get; set; }

    /// <inheritdoc/>
    [StringLength(MaxLengths.SHA384 + 3, MinimumLength = MaxLengths.SHA384)]
    public string? SHA384 { get; set; }

    /// <inheritdoc/>
    [StringLength(MaxLengths.Url)]
    [Comment("文件路径")]
    public string? FilePath { get; set; }

    /// <inheritdoc/>
    [StringLength(MaxLengths.FileExtension)]
    [Comment("文件后缀名")]
    public string? FileExtension { get; set; }

    /// <inheritdoc/>
    [Comment("文件类型")]
    public CloudFileType FileType { get; set; }

    /// <inheritdoc/>
    [Comment("文件大小")]
    public long FileSize { get; set; }

    #endregion

    /// <summary>
    /// 下载地址
    /// </summary>
    [Comment("下载地址")]
    [StringLength(MaxLengths.Url)]
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// 下载方式
    /// </summary>
    [Comment("下载方式")]
    public DownloadMethod DownloadMethod { get; set; }

    /// <summary>
    /// 下载渠道
    /// </summary>
    [Comment("下载渠道")]
    public UpdateChannelType DownloadChannel { get; set; }

    /// <summary>
    /// 下载次数
    /// </summary>
    [Comment("下载次数")]
    public int DownloadCount { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 客户端版本构建
    /// </summary>
    public virtual AppVerBuild Build { get; set; } = null!;

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTimeOffset? Published { get; set; }

    /// <summary>
    /// 静态资源
    /// </summary>
    public virtual StaticResource StaticResource { get; set; } = null!;

    /// <summary>
    /// 适用版本
    /// </summary>
    /// <remarks>
    /// 表示可通过此包更新的版本；如果未指定，则表示此包为全量更新包，所有版本可使用此包更新
    /// </remarks>
    public virtual AppVer ApplicableAppVer { get; set; } = null!;

    ///// <summary>
    ///// 需要的 .NET 运行时版本
    ///// </summary>
    //public virtual DotNetVersion RequiredDotNetVersion { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<AppVerFile>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<AppVerFile> builder)
        {
            builder
                .HasOne(x => x.StaticResource)
                .WithMany()
                .HasForeignKey(x => x.StaticResourceId)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .HasOne(x => x.ApplicableAppVer)
                .WithMany()
                .HasForeignKey(x => x.ApplicableAppVerId)
                .OnDelete(DeleteBehavior.SetNull);

            //builder
            //    .HasOne(x => x.RequiredDotNetVersion)
            //    .WithMany()
            //    .HasForeignKey(x => x.RequiredDotNetVersionId)
            //    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}