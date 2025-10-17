using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;

/// <summary>
/// 静态资源表实体类
/// </summary>
[Table("StaticResources")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class StaticResource :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    ICloudFileInfo
{
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
    /// 访问地址，可为 <see langword="null"/>，像图片之前使用的是 api/image/{id} 的固定规则
    /// </summary>
    [Comment("访问地址")]
    [StringLength(MaxLengths.Url)]
    public string? Url { get; set; }

    /// <summary>
    /// 关联的上传记录
    /// </summary>
    public virtual List<StaticResourceUploadRecord> StaticResourceUploadRecords { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<StaticResource>
    {
        public sealed override void Configure(EntityTypeBuilder<StaticResource> builder)
        {
            base.Configure(builder);
        }
    }
}